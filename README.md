# LiveAuth

LiveAuth is a next-generation authorization model that fixes JWT's biggest flaws:
revocation, stale claims, and distributed logout.

## Why LiveAuth?
JWT tokens are stateless. Once issued, they cannot be revoked or updated.
LiveAuth turns tokens into live references.

## How it works
JWT contains only:
- sid (session id)
- ver (permission version)

All permissions live in a Central Token State Store (CTSS).

## Install
```bash
dotnet add package LiveAuth
