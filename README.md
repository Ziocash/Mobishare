# 🚲 Mobishare
**A smart and green solution to move around the city.**

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-blue) ![Platform](https://img.shields.io/badge/platform-Web%20App-lightgrey) ![License](https://img.shields.io/badge/License-Apache%202.0-green) ![Status](https://img.shields.io/badge/status-In%20Development-orange) ![Made with ❤️](https://img.shields.io/badge/made%20with-%E2%9D%A4-red)

---

**MobiShare** is a smart urban mobility platform that lets you get around the city easily and sustainably using **standard bikes**, **electric bikes**, and **electric scooters**. With MobiShare, you can skip the traffic, reduce your carbon footprint, and move freely through the city.

---

## 🚀 Features

- 🗺 **Live map** showing available vehicles in real time
- 🅿️ **Smart parking assistance** with geofenced zones  
- 🔄 **Ride history & payment tracking**  
- 💳 **In-app payments** with cards or digital wallets  
- 🎫 **Passes & promotions** for frequent users
- 🤖 **AI agent** to improve app usability  

---

## 🚘 Available Vehicles

| 🚴‍♂️ Vehicle               | 📋 Description                                                               |
|--------------------------|------------------------------------------------------------------------------|
| **Standard Bicycle**     | Classic bike without motor assistance — great for short rides and exercise   |
| **Electric Bicycle**     | Pedal-assist bike, perfect for longer distances or hilly routes              |
| **Electric Scooter**     | Fast and flexible option for busy urban streets                              |

---

## 🧑‍💻 Tech Stack

- **Frontend**: Razor Pages, CSS  
- **Backend**: ASP .NET Core
- **Deployment**: Docker compose
- **Database**: SQLite
- **Real-time Communication**: SignalR
- **Authentication**: Google OAuth 2.0
- **Payments**: PayPal SDK
- **Messaging**: MQTT (for vehicle telemetry)
- **Mapping & GPS**: Google Maps SDK  
- **Cloud Services**: AWS
- **Arduino code**: ino
  
---

## 🧾 Required Environment Variables / App Settings

Configure the following variables in your `appsettings.json` as environment variables.

### 🔑 Strucrure of `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../Mobishare.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Ollama": {
    "Llms": {
      "DefaultUrlApiClient": "http://localhost:11434",
      "Qwen3": 
      {
        "UrlApiClient": "http://localhost:11434",
        "ModelName": "qwen3:latest"
      }
    },
    "Embedding": {
      "UrlApiClient": "http://localhost:11434",
      "ModelName": "nomic-embed-text" 
    }
  }
}
```

### 🧪 Example for `secrets`

You must add this file inside of route file, not inside the project.

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "GoogleMaps": {
    "ApiKey": "your-google-maps-api-key"
  },
  "Payments": {
    "PayPal": {
      "ClientId": "your-paypal-client-id",
      "ClientSecret": "your-paypal-client-secret",
      "PayPalUrl": "https://api.sandbox.paypal.com"
    }
  }
}
```

---

## ⚙️ Getting Started

Follow these steps to run the project locally.

### 📥 1. Clone the repository

```bash
git clone https://github.com/your-username/mobishare.git
cd mobishare
```

### 🔧 2. Configure environment variablesClone the repository
Make sure to set up the required settings either in:
- `appsettings.json` and `secrets`.
- Refer to the [Required Environment Variables / App Settings](#-required-environment-variables--app-settings) section for details.

### 📦 3.1.1 Restore dependencies
Restore NuGet packages and all dependencies:
`dotnet restore`

### 🚀 3.1.2 Run the backend
You are ready to go! Run:
`dotnet run --project Mobishare`

###  3.2 🚀 3.1.2 Run with Docker
Easy to run with:
`docker compose up --build`

---

## 🚦 How to Use the Project

### 👤 User Workflow

1. **🏠 Access without login**  
   - Users see only the home page presenting MobiShare and its features.

2. **🔑 User login**  
   - Login via Google OAuth.
   - Redirected to the landing page.

3. **🗺 Landing page**  
   - View all available vehicles (bikes and scooters) on the live map.  
   - Check ride history and account info.
   - Use our custom AI agent.

4. **📅 Book a vehicle**  
   - Reserve a vehicle for a limited time.  
   - Reservation expires → vehicle becomes available again.

5. **💳 Wallet & Payments**  
   - Must have a minimum balance of €5 to start a ride.  
   - Payments handled via PayPal.  
   - First 30 minutes of each ride are free!

6. **🚴‍♂️ Start & end a ride**  
   - Rides must start/end within designated parking areas on the map.

7. **⭐ Points system**  
   - Earn points for rides and sustainable usage.
  
8. **🤖AI agent**
   - Help you to undestrand what the application do.
   - Help you to open tickets, reserve vehicle and much more. 

---

### 🛠 Staff & Admin Workflow

- **👷 Technical users**  
  - Manage vehicle maintenance and support tickets.

- **🗺 Staff**  
  - Manage maps, geofenced zones, and vehicle fleet.

- **👑 Admin**  
  - Full control: user suspension, vehicle management, payments, system oversight.

---

By following this workflow, everyone can enjoy a smooth and smart urban mobility experience with MobiShare! 🚲⚡

---

## 🙏 Credits

This project was developed by the dedicated Mobishare team:

- **BeastOfShadow** – Full Stack Developer, AI specialist | [GitHub](https://github.com/BeastOfShadow) | [LinkedIn](https://www.linkedin.com/in/negro-simone-babb88238/)
- **Cosimo Daniele** – Full Stack Developer, AI specialist | [GitHub](https://github.com/The-Forest03) | [LinkedIn](https://www.linkedin.com/in/cosimo-daniele-a24a13238/)
- **Matteo Schintu** – UX/UI Designer | [GitHub](https://github.com/SkennyCMD) 

Special thanks to everyone who contributed ideas, feedback, and testing.

---

## 📄 License

This project is licensed under the [Apache License 2.0](LICENSE).
