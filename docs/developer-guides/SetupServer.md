# Setup Server

This guide covers the initial configuration of a clean Ubuntu 25.04 server for hosting Byakko in production, including system updates, firewall setup, and VPN access.

## System Preparation

Download the latest updates:

```bash
sudo apt update && sudo apt upgrade -y
```

Install networking utilities:

```bash
sudo apt install net-tools -y
```

## VPN Setup

Follow the [OpenVPN installation guide](https://openvpn.net/community-resources/how-to/) and transfer your client profile to the server:

```bash
scp client.ovpn username@<server-ip>:~/
```

Connect to the VPN before continuing with the firewall configuration.

## Firewall Configuration

Open the required ports using UFW:

```bash
# Public ports
sudo ufw allow 22/tcp      # SSH
sudo ufw allow 80/tcp      # HTTP
sudo ufw allow 443/tcp     # HTTPS
sudo ufw allow 1194/udp    # OpenVPN
sudo ufw allow 16443/tcp   # Kubernetes Deployment

# Restrict Kubernetes Dashboard to VPN subnet only
sudo ufw allow from 10.8.0.0/24 to any port 10443

sudo ufw enable
```

Verify the rules are active:

```bash
sudo ufw status verbose
```

## Remote Access

Once the VPN is running, connect to the server via its VPN IP:

```bash
ssh username@10.8.0.1
```

Restrict SSH to the VPN subnet for additional security by editing `/etc/ssh/sshd_config` or adding a UFW rule that denies SSH from outside `10.8.0.0/24`.

## Next Step

With the server prepared, proceed to the [Kubernetes](Kubernetes.md) guide to install k3s and deploy Byakko.