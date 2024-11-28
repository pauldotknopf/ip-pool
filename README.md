# IpPool

A tool to manage/reserve IP (CIDR notated) from an address space (pool).

# Installation

```bash
dotnet tool install -g IpPool
```

# Usage

```bash
> ip-pool create-pool --state-file state.json --pool 127.0.0.1/8
> ip-pool reserve-ip --state-file state.json --pool 127.1.0.0/24 --key app-services-subnet
reserved: 127.1.0.0/24
> ip-pool reserve-size --state-file state.json --size 8 --key private-endpoint-subnet
reserved: 127.0.0.0/24
```

**```state.json```**

```json
{
  "pool": "127.0.0.0/8",
  "reserved": {
    "app-services-subnet": "127.1.0.0/24",
    "private-endpoint-subnet": "127.0.0.0/24"
  }
}
```