CREATE DATABASE IF NOT EXISTS robostrike;
USE robostrike;

CREATE TABLE IF NOT EXISTS Users (
                                     Id INT AUTO_INCREMENT PRIMARY KEY,
                                     Username VARCHAR(255) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Is_email_validated BOOLEAN NOT NULL DEFAULT FALSE,
    Passwordhash VARCHAR(255) NOT NULL,
    Salt VARCHAR(255) NOT NULL,
    Points INT NOT NULL DEFAULT 0
    );

CREATE TABLE IF NOT EXISTS Sessions (
                                        SessionId VARCHAR(100) PRIMARY KEY,
    UserId INT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

--(replace 'password123' with smth else)
CREATE USER IF NOT EXISTS 'user'@'localhost' IDENTIFIED BY 'password123';
GRANT SELECT, INSERT, UPDATE, DELETE
      ON robostrike.*
          TO 'user'@'localhost';

FLUSH PRIVILEGES;
