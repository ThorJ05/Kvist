SO FAR

Login Platform and Infrastacture for Kvist & Bjælke ApS

What we have built so far

We have coded the core of the login platform in ASP.net core web api with core identity and JWT authentiaction, together with a Bun + React frontend. The platform itself is not yet launched to the company's server, but the security aspect is mostly done while the infr. still needs some code to be done.

The CDS Security side. More or less done.

it meets the minium requirements for example:

- Username/password authentication via our AuthController with the register, login, me.
- Role based auth. with three requried roles, like Users can only read on users, SuperUsers has CRUD on the user accounts only and our Admin this has CRUD on all users including Admins.
- Self-registration. Users can't pick their own role this has to be choosen by the admin or superuser
- Password are never stored in plaintext thanks to our hashing
- Web based access. Dockerisation is still underway.


CDS Infra

NOT YET STARTED.



USE OF AI SO FAR

We used AI assitance like claude, Copilot and deepseek.

We used it here(code): Drafting part of AuthController.cs and UserController.cs.



















# bun-react-template

To install dependencies:

```bash
bun install
```

To start a development server:

```bash
bun dev
```

To run for production:

```bash
bun start
```

This project was created using `bun init` in bun v1.3.14. [Bun](https://bun.com) is a fast all-in-one JavaScript runtime.
