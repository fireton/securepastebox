# SecurePasteBox

SecurePasteBox is a lightweight and secure application for one-time sharing of encrypted messages. Secrets are encrypted entirely in the browser and never transmitted to the server in plain text.

## Features

* Client-side encryption using AES-GCM
* The server stores only a one-time key, which is deleted after the first read
* The encrypted message is embedded in the URL fragment and never reaches the server
* Minimal interface with dark theme and orange accent
* Docker-ready for easy deployment

## Quick Start (Docker)

```bash
docker build -t securepastebox .
docker run -p 8080:80 securepastebox
```

Then open: `http://localhost:8080`

## Project Structure

* `Pages/` – static frontend (HTML, JS, Pico CSS)
* `Program.cs` – minimal .NET backend with key API
* `IKeysManager` – interface for key storage (e.g., in-memory, Redis)

## License

MIT
