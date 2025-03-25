# OpenFGADemo

A demonstration project showcasing how to implement OpenFGA (Fine-Grained Authorization) in .NET applications.

## About OpenFGA

[OpenFGA](https://openfga.dev/) is an open-source Fine-Grained Authorization system based on Google's Zanzibar paper. It provides a flexible, high-performance authorization solution that can handle complex permission scenarios at scale.

Key features of OpenFGA:

- Relationship-based authorization model
- Scalable and efficient permission checking
- Support for complex authorization relationships
- Language-agnostic with multiple client SDKs

## Project Overview

This demo demonstrates how to:

- Integrate OpenFGA with a .NET application
- Define authorization models
- Create and manage authorization relationships
- Perform permission checks

The project implements a simple document management scenario where:

- Documents can have readers, writers, and owners
- Different users can be assigned different permission levels
- Authorization checks validate user access to documents

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/DogusTeknoloji/OpenFGADemo.git
cd OpenFGADemo
```

### 2. Start the OpenFGA server

The project includes a Docker Compose configuration to run the OpenFGA server locally with PostgreSQL:

```bash
cd docker
docker-compose up -d
```

This will start:

- PostgreSQL database on port 5432
- OpenFGA server on port 8080
- OpenFGA UI playground on port 3000

### 3. Run the .NET application

```bash
cd src/OpenFGADemo
dotnet run
```

By default, the application will run on `https://localhost:7069` and `http://localhost:5234`.

## Using the Demo

The project includes several API endpoints demonstrating OpenFGA functionality:

### List OpenFGA Stores

```http
GET /documents/list-store
```

### Create OpenFGA Store

```http
PUT /documents/create-store
```

### Create Authorization Model

```http
PUT /documents/create
```

### Assign Permissions

```http
POST /documents/assign?userId={userId}&documentId={documentId}&permission={permission}
```

Example:

```http
POST /documents/assign?userId=alice&documentId=report.pdf&permission=reader
```

### Check Permissions

```http
POST /documents/check
Content-Type: application/json

{
  "userId": "alice",
  "documentId": "report.pdf",
  "permission": "reader"
}
```

## How It Works

This demo uses the OpenFGA .NET SDK to interact with an OpenFGA server. The application:

1. Connects to the OpenFGA server using the client configuration in `Program.cs`
2. Defines an authorization model in `Models/AuthorizationModel.cs`
3. Provides APIs to manage and check permissions in `Controllers/DocumentsController.cs`

### Authorization Model

The demo uses a simple model with users and documents:

```OpenFGA DSL
type user

type document
  relations
    define reader: [user]
    define writer: [user]
    define owner: [user]
```

## Project Structure

- `/docker` - Docker Compose setup for OpenFGA and PostgreSQL
- `/src` - .NET application source code
  - `/Controllers` - API controllers
  - `/Models` - Authorization model definitions

## Technologies Used

- .NET 9.0
- OpenFGA SDK for .NET
- Docker & Docker Compose
- PostgreSQL

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## Resources

- [OpenFGA Documentation](https://openfga.dev/docs)
- [Zanzibar Paper](https://research.google/pubs/pub48190/)
- [OpenFGA GitHub Repository](https://github.com/openfga/openfga)
