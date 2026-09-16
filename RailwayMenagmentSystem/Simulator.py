import time
import json
import urllib.request
import urllib.error

API_URL = "http://localhost:5089/api/trainlocation"

trains = {
    4: {
        "name": "Vardar Express",
        "route": [7, 8, 9, 13, 14, 15]
    },
    5: {
        "name": "Balkan Star",
        "route": [15, 14, 13, 9, 8, 7]
    },
    7: {
        "name": "Skopje Express",
        "route": [7, 8, 9, 10, 11, 12]
    },
    6: {
        "name": "Makedonija Rail",
        "route": [12, 11, 10, 9, 8, 7]
    }
}

positions = {
    4: 0,
    5: 0,
    7: 0,
    6: 0
}

print("Starting Railway Train Location Simulator...")
print(f"Target Endpoint: {API_URL}\n")

while True:
    for train_id, train in trains.items():

        station_id = train["route"][positions[train_id]]

        payload = {
            "trainId": train_id,
            "stationId": station_id
        }

        json_data = json.dumps(payload).encode("utf-8")

        req = urllib.request.Request(
            API_URL,
            data=json_data,
            headers={"Content-Type": "application/json"},
            method="POST"
        )

        try:
            with urllib.request.urlopen(req) as response:
                response_body = response.read().decode("utf-8")
                server_reply = json.loads(response_body)

                print(
                    f"[SENT] {train['name']} | "
                    f"Train ID: {train_id} | "
                    f"Station ID: {station_id} | "
                    f"Server: {server_reply.get('message')}"
                )

        except urllib.error.HTTPError as e:
            print(
                f"[SERVER ERROR] {train['name']} | "
                f"HTTP {e.code} | "
                f"{e.read().decode('utf-8')}"
            )

        except urllib.error.URLError:
            print(
                f"[CONNECTION ERROR] "
                f"Is the ASP.NET Core application running?"
            )

        positions[train_id] += 1

        if positions[train_id] >= len(train["route"]):
            positions[train_id] = 0

    print("\n--- Next update ---\n")
    time.sleep(10)