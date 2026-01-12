# API Key Auth Demo

This demo provides:

- A minimal Node.js server with username/password login and session-based admin UI
- An admin UI to create API keys
- An endpoint `/api/validate-key` to validate keys (for use by external programs)
- A sample C# console client that calls the validation endpoint

Quick start

1. Clean and install Node dependencies:

```bash
cd /workspace/apikey-auth-demo
# If previous install failed, clean first:
# rm -rf node_modules package-lock.json
npm install
```

2. Start the server:

```bash
npm start
```

3. Open the admin UI in your browser:

- `http://localhost:3000/login.html`
- Default credentials: `admin` / `admin` (change in production)

4. Create a key from the admin page. Use the created key in your program.

C# sample client

```bash
cd csharp-client
dotnet run -- <API_KEY>
```

Notes

- This is a demo: do not use the default secret or session store in production.
- Persisted data is stored in `data.sqlite` in the project folder.
- You can call `POST /api/validate-key` with header `X-Api-Key: <key>` to validate keys.
