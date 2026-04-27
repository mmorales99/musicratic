# Demo Runbook

## Goal

Show the shop owner and visitor flows working end to end in the browser.

## Start

1. Open a terminal in the `src` folder.
2. Run the host.

```powershell
dotnet run --project .\Host\Musicratic.Host\Musicratic.Host.csproj --launch-profile http
```

3. Open `http://localhost:5258/`.

## Demo Flow

### 1. Home Page

Show the landing page first.

Point out that the app has two test surfaces:

- Shop owner
- Visitor

### 2. Shop Owner

Open the shop owner page.

Use it to show:

- runtime states are available
- the current session payload loads
- the owner can start the session
- the owner can close the session

Mention that the live state switches to Live when start is clicked.

### 3. Visitor

Open the visitor page.

Use it to show:

- auth status is visible
- song catalog items load from the backend
- pending queue data is visible
- a song suggestion updates the queue

If needed, click Suggest on the first song to show a queue update.

## Talking Points

- The backend and UI are integrated in one host.
- The owner controls the session lifecycle.
- The visitor sees the catalog, auth state, and pending queue.
- Song suggestions flow through the backend APIs.

## Reset

If the demo needs a clean state, stop and restart the host.

Because the current implementation uses in-memory state, a restart resets the demo data.
