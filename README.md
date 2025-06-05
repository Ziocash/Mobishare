# WORK IN PROGRESS... EMPOWERING CHATBOT WITH TOOL AI MODEL... GREAT STUFF IS COOKING HERE! LET US COOK AND TRUST THE PROCESS!
# 🚲 Mobishare
**A smart and green solution to move around the city.**

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-blue) ![Platform](https://img.shields.io/badge/platform-Web%20App-lightgrey) ![License](https://img.shields.io/badge/License-Apache%202.0-green) ![Status](https://img.shields.io/badge/status-In%20Development-orange) ![Made with ❤️](https://img.shields.io/badge/made%20with-%E2%9D%A4-red)

---

**MobiShare** is a smart urban mobility platform that lets you get around the city easily and sustainably using **standard bikes**, **electric bikes**, and **electric scooters**. With MobiShare, you can skip the traffic, reduce your carbon footprint, and move freely through the city.

---

## 🚀 Features

- 🗺 **Live map** showing available vehicles in real time
- ⚡ **Battery level monitoring** (for e-bikes and scooters)  
- 🅿️ **Smart parking assistance** with geofenced zones  
- 🔄 **Ride history & payment tracking**  
- 💳 **In-app payments** with cards or digital wallets  
- 🎫 **Passes & promotions** for frequent users  

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
- **Database**: SQLite
- **Real-time Communication**: SignalR
- **Authentication**: Google OAuth 2.0
- **Payments**: PayPal SDK
- **Messaging**: MQTT (for vehicle telemetry)
- **Mapping & GPS**: Google Maps SDK  
- **Cloud Services**: AWS
  
---

## 🧾 Required Environment Variables / App Settings

Configure the following variables in your `appsettings.Development.json`, `appsettings.Production.json`, or as environment variables.

### 🔑 Required Keys

- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`
- `GoogleMaps:ApiKey`
- `Payments:PayPal:ClientId`
- `Payments:PayPal:ClientSecret`
- `Payments:PayPal:PayPalUrl` (e.g. `https://api.sandbox.paypal.com` or `https://api.paypal.com`)

---

### 🧪 Example `appsettings.Development.json`

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
- `appsettings.Development.json` (recommended for local development), or
system environment variables.
- Refer to the [Required Environment Variables / App Settings](#-required-environment-variables--app-settings) section for details.

### 📦 3. Restore dependencies
Restore NuGet packages and all dependencies:
`dotnet restore`

### 🚀 4. Run the backend
You are ready to go! Run:
`dotnet run --project Mobishare`

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

4. **📅 Book a vehicle**  
   - Reserve a vehicle for a limited time.  
   - Reservation expires → vehicle becomes available again.

5. **💳 Wallet & Payments**  
   - Must have a minimum balance of €5 to start a ride.  
   - Payments handled via PayPal.  
   - First 30 minutes of each ride are free!

6. **🚴‍♂️ Start & end a ride**  
   - Rides must start/end within designated parking areas on the map.

7. **⭐ Points system (coming soon)**  
   - Earn points for rides and sustainable usage (to be implemented).

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

- **BeastOfShadow** – Full Stack Developer | [GitHub](https://github.com/BeastOfShadow) | [LinkedIn](https://www.linkedin.com/in/negro-simone-babb88238/)
- **Cosimo Daniele** – Full Stack Developer | [GitHub](https://github.com/The-Forest03) | [LinkedIn](https://www.linkedin.com/in/cosimo-daniele-a24a13238/)
- **Matteo Schintu** – UX/UI Designer | [GitHub](https://github.com/SkennyCMD) 

Special thanks to everyone who contributed ideas, feedback, and testing.

---

## 📄 License

This project is licensed under the [Apache License 2.0](LICENSE).
