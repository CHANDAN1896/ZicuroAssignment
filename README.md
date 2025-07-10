# ZicuroAssignment
ABX Client Packet Reader

A C# TCP client that connects to a server to fetch financial ticker packets, detects missing ones, requests them, and outputs the results to a JSON file.

 # Features
Connects to 127.0.0.1:3000

Reads 17-byte ticker packets

Detects and requests missing sequences

Saves output as output.json

Logs errors to logs/ folder

# Requirements
.NET 6.0 or later

A TCP server that supports:

0x01 0x00 → request all packets

0x02 <sequence> → request specific packet

To open port 3000, you must first install Node.js. After that, extract the provided ZIP file named abx_exchange_server.zip. Inside the extracted folder, you'll find a file named main.js, which is the entry point for the server.

go to cmd (respected file path) hit command : node main.js

# How to Run

git clone --branch master 

https://github.com/CHANDAN1896/ZicuroAssignment.git

Use above Url and Clone the application

Build and run the application


# Output
Parsed packets saved to output.json

Errors logged to logs/YYYY-MM-DD.log

Please Reach Out if you have any query.
chandangupta212001@gmail.com
