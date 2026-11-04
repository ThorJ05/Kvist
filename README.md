# Kvist & Bjælke ApS – Secure Login Platform & Infrastructure

Compulsory assignment, 3rd semester, Computer Science (CDS.security & CDS.infrastructure)

- **Group members:** Thor, Frederik and Pattrick

## Contents

- [Introduction](#introduction)
- [Part 1 – CDS.security](#part-1--cdssecurity)
    - [1.1 Development of the login platform](#11-development-of-the-login-platform)
    - [1.2 Implementation](#12-implementation)
- [Part 2 – CDS.infrastructure](#part-2--cdsinfrastructure)
    - [2.1 The company's network](#21-the-companys-network)
    - [2.2 Development of an IP addressing plan](#22-development-of-an-ip-addressing-plan)
    - [2.3 Establishing a server on VLAN 571](#23-establishing-a-server-on-vlan-571)
    - [2.4 Establishing a client on VLAN 570](#24-establishing-a-client-on-vlan-570)
    - [2.5 Network design](#25-network-design)
    - [2.6 Testing and documentation](#26-testing-and-documentation)
- [Part 3 – Final submission](#part-3--final-submission)
    - [Final reflection](#final-reflection)
    - [AI disclosure](#ai-disclosure)

---

## Introduction

Kvist & Bjælke ApS wants a new login platform where security is considered from the start, and where users’ passwords can never be read in plaintext, even if someone gains access to the database. 
The company also wants to modernise its IT infrastructure with a structured network, where different types of devices are organised into separate networks.


This report documents our solution to both parts of the assignment.

In CDS.security, we developed a web-based login platform with role-based access (user, superuser and admin), built with ASP.NET Core and ASP.NET Core Identity as the backend, JWT for authentication, and a React frontend. 

In CDS.infrastructure, we planned an IP addressing plan with VLANs for the company’s devices, and deployed the login platform in a Docker container on an Ubuntu Server virtual machine on VLAN 571. 
We tested access to it from a client virtual machine on VLAN 570.

The two parts are connected: 
the platform from CDS.security runs on the infrastructure from CDS.infrastructure. 
The report is divided into one section per course, followed by a final reflection and a disclosure of how we used AI.

---

# Part 1 – CDS.security

## 1.1 Development of the login platform

### Description of the developed login platform

The login platform is a web application where users log in with a username and password. 
After logging in, they see a table of all users. 
What a user can do with the table depends on their role: a normal user can only read it, a superuser can manage normal users, and an admin can manage everyone. 
Users cannot choose their own role, and passwords are never stored or shown in plaintext. 
The solution is built to run in a Docker container.



### Technology choices and justification

- **ASP.NET Core Web API with ASP.NET Core Identity:** Identity is a mature, widely used library that handles password hashing, account lockout and roles. 
Using it means we don't write security-critical code ourselves.

- **JWT bearer tokens:** the API is stateless, so the React frontend can call it without server-side sessions. 
The token contains the user's roles, is signed so it can't be altered, and expires after 2 hours.

- **SQLite with Entity Framework Core:** a simple file-based database that is easy to run in Docker. 
EF Core uses parameterised queries, which protects against SQL injection.

- **React (TypeScript) with Bun:** a fast development setup for a simple user interface. 
The frontend only displays data. 
The security rules are enforced by the API.

- **Docker:** the application runs the same way on any server, which makes it easy to deploy on the virtual machine in Part 2.



### Roles and permissions

The platform has three roles. The permissions are enforced on the server, not only in the interface, so hiding a button is not what protects the data.

The screenshots below show the table as each role sees it.

![Admin view](docs/images/admin-view.png)
*Logged in as admin: can edit and delete users and superusers, and create any role.*

![SuperUser view](docs/images/superuser-view.png)
*Logged in as superuser: can only edit and delete users with the role User. The admin and superuser rows have no actions.*

![User view](docs/images/user-view.png)
*Logged in as a normal user: read-only table, no actions and no create form.*

---

## 1.2 Implementation

### Authentication method
TODO

### Password handling and storage
TODO

### Role-based authorization
TODO

### Other security features
TODO

### Alternatives we found along the way
TODO

### Screenshots of key functionality
TODO

### Challenges and solutions
TODO

### Source code
TODO

---

# Part 2 – CDS.infrastructure

## 2.1 The company's network

### Device overview
TODO

### Future expansion
TODO

---

## 2.2 Development of an IP addressing plan

### Logical networks and VLANs
TODO

### Private IP ranges
TODO

### Subnet masks and CIDR notation
TODO

### Default gateways
TODO

### Which devices belong to each network
TODO

### Static IP addresses and DHCP
TODO

### Justification of the network segmentation
TODO

### Subnetting

#### Selected networks
TODO

#### Network address, subnet mask and number of hosts
TODO

#### Why the subnet sizes are appropriate
TODO

#### Room for future expansion
TODO

---

## 2.3 Establishing a server on VLAN 571

### Virtual machine specification
TODO

### Ubuntu Server installation
TODO

### IP configuration
TODO

### Docker installation
TODO

### Deployment of the login platform
TODO

### Chosen port (8000–8100)
TODO

### Verification
TODO

---

## 2.4 Establishing a client on VLAN 570

### Creating the virtual machine
TODO

### Network settings
TODO

### IP configuration check
TODO

### Connectivity to VLAN 571
TODO

### Accessing the login platform
TODO

### Documentation that the platform works from the client
TODO

### Firewall and communication between VLANs

#### Why VLAN 570 and VLAN 571 are separate networks
TODO

#### Why communication between VLANs requires routing
TODO

#### The role of the firewall
TODO

#### Why allowing only specific ports is beneficial
TODO

---

## 2.5 Network design

### Network diagram
TODO

### Explanation of the design
TODO

---

## 2.6 Testing and documentation

### Server – VLAN 571

- Ubuntu Server installed: TODO
- Correct IP configuration: TODO
- Docker installed and working: TODO
- Login platform running in a Docker container: TODO
- Login platform accessible through the selected port: TODO

### Client – VLAN 570

- Correct IP configuration: TODO
- Client can access the login platform on VLAN 571: TODO
- Login platform's functionality can be used from the client: TODO

### Problems encountered and how we solved them
TODO

---

# Part 3 – Final submission

## Final reflection

### How are security and infrastructure interconnected?
TODO

### What technical challenges did we encounter?
TODO

### What did we learn about deploying an application on a server?
TODO

### What would we do differently in a real company?
TODO

---

## AI disclosure

We used AI assistants in this project: **Claude (Anthropic), GitHub Copilot and DeepSeek.** TODO: edit so this matches what each of us really did.

| Area | AI tool | What it was used for | How we checked it |
|---|---|---|---|
| `AuthController.cs`, `UserController.cs` | TODO | Drafting parts of the code | TODO |
| Superuser permission fix, `Program.cs` database folder fix | Claude | Finding the cause of the bug and suggesting the fix | TODO |
| IP addressing plan and subnetting | Claude | Suggesting the VLAN split and example calculations | TODO |
| Proxmox, Ubuntu and Docker setup | Claude | Step-by-step guidance | TODO |
| README structure | Claude | Template and headings | TODO |

**Our own work:** TODO