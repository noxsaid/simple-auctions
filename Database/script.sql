-- Kör med ett konto som får skapa databas/tabeller. Befintlig data raderas inte.
CREATE DATABASE IF NOT EXISTS simple_auctions CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE simple_auctions;

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(254) NOT NULL UNIQUE,
    password_hash VARCHAR(512) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS auctions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    end_time DATETIME(6) NOT NULL COMMENT 'UTC',
    seller_id INT NOT NULL,
    FOREIGN KEY (seller_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS bids (
    id INT AUTO_INCREMENT PRIMARY KEY,
    amount DECIMAL(12,2) NOT NULL,
    time DATETIME(6) NOT NULL COMMENT 'UTC',
    buyer_id INT NOT NULL,
    auction_id INT NOT NULL,
    FOREIGN KEY (buyer_id) REFERENCES users(id),
    FOREIGN KEY (auction_id) REFERENCES auctions(id),
    INDEX auction_amount (auction_id, amount)
) ENGINE=InnoDB;
