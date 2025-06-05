const createSection = document.getElementById('create-section');
const readSection = document.getElementById('read-section');

const baseURL = window.location.origin;

function toBase64Url(bytes) {
    return btoa(String.fromCharCode(...bytes))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, ""); // убираем padding
}

function fromBase64Url(base64url) {
    let base64 = base64url
        .replace(/-/g, "+")
        .replace(/_/g, "/");

    // восстановим padding
    while (base64.length % 4 !== 0) {
        base64 += "=";
    }

    return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
}

const getKeyIdFromPath = () => {
    const match = window.location.pathname.match(/^\/([a-zA-Z0-9-]{8,})$/);
    return match ? match[1] : null;
};

const encryptedFromHash = () => {
    const hash = window.location.hash.slice(1);
    return hash ? fromBase64Url(hash) : null;
};

const encrypt = async (plaintext, key) => {
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const encoded = new TextEncoder().encode(plaintext);
    const ciphertext = await crypto.subtle.encrypt(
        { name: "AES-GCM", iv },
        key,
        encoded
    );
    const combined = new Uint8Array(iv.byteLength + ciphertext.byteLength);
    combined.set(iv, 0);
    combined.set(new Uint8Array(ciphertext), iv.byteLength);
    return toBase64Url(combined);
};

const decrypt = async (data, key) => {
    const iv = data.slice(0, 12);
    const ciphertext = data.slice(12);
    const plaintext = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv },
        key,
        ciphertext
    );
    return new TextDecoder().decode(plaintext);
};

const generateKey = async () =>
    crypto.subtle.generateKey({ name: "AES-GCM", length: 256 }, true, ["encrypt", "decrypt"]);

const exportKey = async (key) =>
    btoa(String.fromCharCode(...new Uint8Array(await crypto.subtle.exportKey("raw", key))));

const importKey = async (base64) =>
    crypto.subtle.importKey("raw", Uint8Array.from(atob(base64), c => c.charCodeAt(0)), "AES-GCM", true, ["encrypt", "decrypt"]);

const sendKeyToServer = async (base64Key) => {
    const res = await fetch('/api/keys', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ key: base64Key })
    });
    if (!res.ok) throw new Error('Failed to store key');
    const { keyId } = await res.json();
    return keyId;
};

const getKeyFromServer = async (keyId) => {
    const res = await fetch(`/api/keys/${keyId}`, { method: 'DELETE' });
    if (!res.ok) throw new Error('Key not found or already used');
    const keydata = await res.json();
    return keydata.key;
};

// Logic
const keyId = getKeyIdFromPath();
const encrypted = encryptedFromHash();

if (keyId && encrypted) {
    readSection.hidden = false;

    document.getElementById('reveal-button').addEventListener('click', async () => {
        try {
            const base64Key = await getKeyFromServer(keyId);
            const key = await importKey(base64Key);
            const secret = await decrypt(encrypted, key);
            document.getElementById('output').value = secret;
            document.getElementById('output-container').hidden = false;
        } catch (e) {
            alert('Failed to reveal the secret: ' + e.message);
        }
    });

    document.getElementById('copy-secret').addEventListener('click', () => {
        navigator.clipboard.writeText(document.getElementById('output').value);
    });

} else {
    createSection.hidden = false;

    document.getElementById('create-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const plaintext = document.getElementById('secret').value;
        const key = await generateKey();
        const base64Key = await exportKey(key);
        const keyId = await sendKeyToServer(base64Key);
        const encrypted = await encrypt(plaintext, key);
        const link = `${baseURL}/${keyId}#${encrypted}`;
        const input = document.getElementById('secure-link');
        input.value = link;
        document.getElementById('link-container').hidden = false;
    });

    document.getElementById('copy-link').addEventListener('click', () => {
        navigator.clipboard.writeText(document.getElementById('secure-link').value);
    });
}