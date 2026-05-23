# Ubuntu Dev Environment

This guide covers the one-time setup steps required to run Byakko locally on Ubuntu 25.04.

## Setup Dev HTTPS Certificates

Install the `linux-dev-certs` tool and create locally-trusted development certificates:

```shell
dotnet tool update -g linux-dev-certs
dotnet linux-dev-certs install
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

This installs a locally-trusted root certificate so the HTTPS endpoints (e.g. `https://localhost:7286`) are trusted by your browser without certificate warnings.

## Give Docker User Rights

Add your user to the `docker` group so you can run Docker commands without `sudo`:

```shell
sudo groupadd docker
sudo usermod -aG docker $USER
newgrp docker
sudo systemctl restart docker
```

After running these commands, log out and back in (or restart your shell) for the group membership to take full effect.