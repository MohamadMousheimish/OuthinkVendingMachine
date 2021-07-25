# Outhink Technical Task Documentation

## Backend Project

Technical task is about building a vending machine, how I thought about solving the issue.
Vending machine is a one type operation:

- Can receive one coin per insertion
- Coins are being cached in a in-memory and not stored in database table (no need to)
- When process can be done successfully, coins are taken from cache and added to vending machine initial coins
- If process can't be done (due to known condition) cached coins will be returned to user

How I thought about the coin returning process:

- 1 euro are handled as 100 cent.
- When process can be done, add cached coins value.
- To get number of coins returned, get the quotient of coin value (100, 50, 20 and 10).
- To get the remainng value of each coin, get the reminder of coin value `modulo` (100, 50, 20, 10).
- For each coin, check if returned value exist in the vending machine, if not, add the remaining to the following smaller value (if 1 euro don't exist, add two 50 cent to the returned fifties).

## Features

- User can insert coins to the vending machine.
- User can cancel the buy operation and take back the inserted coins.
- User can buy a specific item in the vending.
- `Thank you` message will be displayed if inserted coins is the exact price of bought item.
- `Thank you` message will be displayed in addition for price difference if inserted coins are bigger than bought item price.
- `Insufficient amount` message will be displayed if inserted coins are less than bought item price, in addition for inserted coins.
- `Please insert coins first!` message will be displayed if no coins are inserted and user try to buy.
- `Item sold out` message will be displayed if vending machine doesn't have portions of the specified bought item anymore.

## APIs

- Insert Coin: API to insert coins
- Cancel Order: API to cancel an order and return the inserted coins
- Buy Order: API to buy an order with the inserted coins
- Get Items: API to get items from vending machine (to be displayed in the list)
- Get Coins: API to get the coins in the vending machine (to be displayed and seen in the UI - for testing purpose only)

## Frontend Project

Frontend project is a simple UI in which it have the following UI components

- List to select a coin you want to insert
- List that contain the existed vending machine items
- When you want to insert a coin, select a coin from the select list and click on `Insert Coin` button
- When you want to cancel an order, click on the `Cancel` button
- When you want to buy a specific item, select an item from the items list and click on `Buy Item` button
- When an item is bought successfully, a message from vending machine will be displayed, in addition for the returned coins (if vending machine returned any)
- Simple texts to display the coins in the vending machine (to show how coins are being added and substracted)

## Installation

For the project to run, please work with the following steps:

- Clone the backend github repository
- Open .sln project
- Press Ctrl+F5, F5 or click on the run icon in visual studio
- Clone the frontend github repository
- Enter the cloned folder

```sh
cd folderName
```

- Install the dependencies and devDependencies.

```sh
npm i (or npm install)
```

- Start the angular application

```sh
ng serve
```

- Open browser and go to http://localhost:4200
