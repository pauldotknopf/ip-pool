# IpPool

A tool to manage/reserve IPs (CIDR notated) from an address space (pool).

# Installation

```bash
dotnet tool install -g IpPool
dotnet tool update -g IpPool
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

You can also optionally generate a ```.tf``` file, which makes integration into Terraform a lot easier.

```bash
ip-pool generate-tf --state-file state.json
```

**```state.tf```**

```terraform
locals {
	app-services-subnet = "127.1.0.0/24"
	private-endpoint-subnet = "127.0.0.0/24"
}
```
