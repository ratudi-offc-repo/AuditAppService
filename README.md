# AuditAppService

## Overview

AuditAppService is a service designed to facilitate and manage audit operations within the application. It provides APIs and utilities to record, query, and analyze audit events, ensuring accountability and compliance.

## Features

- Record audit events with detailed metadata
- Query audit logs by user, action, or time period

## Installation

```bash
# Clone the repository
git clone https://github.com/ratudi-offc-repo/AuditAppService.git

# Navigate to the project directory
cd AuditAppService
```
open the solution file and run the project


## API Endpoints

| Method | Endpoint                | Description                   |
|--------|------------------------ |------------------------------|
| POST   | /audit/events           | Record an audit event         |
| GET    | /audit/events           | List or query audit events    |
| GET    | /audit/events/{id}      | Get details of a specific event|

