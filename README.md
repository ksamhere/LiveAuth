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

```
## Run Test samples
Run the WebApiSample project in which the LiveAuth is being used and then run the LiveAuth.TestClient project.

In this TestClient project, you can see there are few test cases with one valid session id and details and remaining invalid session id are used.

Refer the below image to understand the output for the following 

Testcases:

<img width="875" height="217" alt="image" src="https://github.com/user-attachments/assets/8faa626b-ec22-47c1-a818-cfe63d12b4f4" />

Output:

<img width="718" height="410" alt="image" src="https://github.com/user-attachments/assets/b13857b1-4c45-4d70-87e8-9396164e5f29" />

