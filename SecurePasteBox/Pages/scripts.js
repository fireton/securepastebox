class UserFriendlyError extends Error {
    constructor(message) {
        super(message);
        this.name = "UserFriendlyError";
    }
}

const createSection = document.getElementById('create-section');
const readSection = document.getElementById('read-section');
const errorSection = document.getElementById('error-section');

const baseURL = window.location.origin;

function toBase64Url(bytes) {
    return btoa(String.fromCharCode(...bytes))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, ""); // removing padding
}

function fromBase64Url(base64url) {
    try {
        let base64 = base64url
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        // restoring padding
        while (base64.length % 4 !== 0) {
            base64 += "=";
        }

        return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
    }
    catch {
        throw new UserFriendlyError("The link seems broken or contains invalid characters.")
    }
}

const getKeyIdFromPath = () => {
    const match = window.location.pathname.match(/^\/([a-zA-Z0-9-]{8,})$/);
    return match ? match[1] : null;
};

const generateHash = async (encrypted) => {
    const digest = await crypto.subtle.digest("SHA-256", encrypted);
    return toBase64Url(new Uint8Array(digest)).slice(0, 8);
}

const encryptedFromHash = async () => {
    const hashPart = window.location.hash.slice(1);
    if (!hashPart) return null;

    const [encryptedBase64, hash] = hashPart.split('.');

    if (!encryptedBase64 || !hash) {
        throw new UserFriendlyError("The link seems broken or incomplete. Please check and try again.");
    }

    const encrypted = fromBase64Url(encryptedBase64);
    const computedHash = await generateHash(encrypted);
    if (computedHash !== hash) {
        throw new UserFriendlyError("This link doesn’t look valid. It may be incomplete or has been changed.");
    }

    return encrypted;
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
    return combined;
};

const decrypt = async (data, key) => {
    try {
        const iv = data.slice(0, 12);
        const ciphertext = data.slice(12);
        const plaintext = await crypto.subtle.decrypt(
            { name: "AES-GCM", iv },
            key,
            ciphertext
        );
        return new TextDecoder().decode(plaintext);
    }
    catch {
        throw new UserFriendlyError("We couldn’t read this secret. It may have been damaged or changed.");
    }
};

const generateKey = async () =>
    crypto.subtle.generateKey({ name: "AES-GCM", length: 256 }, true, ["encrypt", "decrypt"]);

const exportKey = async (key) =>
    btoa(String.fromCharCode(...new Uint8Array(await crypto.subtle.exportKey("raw", key))));

const importKey = async (base64) =>
    crypto.subtle.importKey("raw", Uint8Array.from(atob(base64), c => c.charCodeAt(0)), "AES-GCM", true, ["encrypt", "decrypt"]);

const sendKeyToServer = async (base64Key, expiration = "7.00:00:00") => {
    const res = await fetch('/api/keys', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ key: base64Key, expiration })
    });
    if (!res.ok) throw new UserFriendlyError('We couldn’t save your secret. Please try again in a moment.');
    const { keyId } = await res.json();
    return keyId;
};

const getKeyFromServer = async (keyId) => {
    const res = await fetch(`/api/keys/${keyId}`, { method: 'DELETE' });
    if (!res.ok) throw new UserFriendlyError('This secret is no longer available. It may have already been revealed or has expired.');
    const keydata = await res.json();
    return keydata.key;
};

const showError = (message) => {
    document.getElementById('create-section').hidden = true;
    document.getElementById('read-section').hidden = true;
    document.getElementById('error-section').hidden = false;
    const errorText = document.getElementById('error-message');
    errorText.textContent = message;
};

const handleException = (e) => {
    if (e instanceof UserFriendlyError) {
        showError(e.message);
    } else {
        console.error(e);
        showError("An unexpected error occurred. Please try again.");
    }
};

const processCreate = async () => {
    createSection.hidden = false;
    document.getElementById('secret').value = '';
    document.getElementById('link-container').hidden = true;
    document.getElementById('create-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            const plaintext = document.getElementById('secret').value;
            const key = await generateKey();
            const base64Key = await exportKey(key);
            const expiration = document.getElementById('expiration').value;
            const keyId = await sendKeyToServer(base64Key, expiration);
            const encrypted = await encrypt(plaintext, key);
            const hashBase64 = await generateHash(encrypted);
            const encryptedBase64 = toBase64Url(encrypted);
            const link = `${baseURL}/${keyId}#${encryptedBase64}.${hashBase64}`;
            const input = document.getElementById('secure-link');
            input.value = link;
            document.getElementById('link-container').hidden = false;
        } catch (e) {
            handleException(e);
        }
    });

    document.getElementById('copy-link').addEventListener('click', () => {
        navigator.clipboard.writeText(document.getElementById('secure-link').value);
    });
}

const processReveal = async (keyId, encrypted) => {
    readSection.hidden = false;

    document.getElementById('reveal-button').addEventListener('click', async () => {
        try {
            const base64Key = await getKeyFromServer(keyId);
            const key = await importKey(base64Key);
            const secret = await decrypt(encrypted, key);
            document.getElementById('output').value = secret;
            document.getElementById('reveal-section').hidden = true;
            document.getElementById('output-container').hidden = false;
        } catch (e) {
            handleException(e);
        }
    });

    document.getElementById('copy-secret').addEventListener('click', () => {
        navigator.clipboard.writeText(document.getElementById('output').value);
    });
}

// Logic
(async () => {
    const keyId = getKeyIdFromPath();
    let encrypted = null;

    try {
        encrypted = await encryptedFromHash();
    }
    catch (e) {
        handleException(e);
        return;
    }

    if (keyId && encrypted) {
        await processReveal(keyId, encrypted);
    } else {
        await processCreate();
    }
})();