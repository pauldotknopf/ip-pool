# IpPool

A tool to manage/reserve IPs (CIDR notated) from an address space (pool).

# Installation

```bash
dotnet tool install -g IpPool
dotnet tool update -g IpPool
```

# Usage

```bash
# First, create your global address space
ip-pool create-pool --state-file state.json --pool 127.0.0.1/8

# Reserve two vnets, with 22 bits of addressable space (4,194,304 IPs)
> ip-pool reserve-vnet --state-file state.json --size 22 --key region1
reserved: 127.0.0.0/10
> ip-pool reserve-vnet --state-file state.json --size 22 --key region2
reserved: 127.64.0.0/10

# Reserve snets in each of those vnets
# region 1
> ip-pool reserve-subnet --state-file state.json --vnet-key region1 --key my-snet1 --size 8
reserved: 127.0.0.0/24
> ip-pool reserve-subnet --state-file state.json --vnet-key region1 --key my-snet2 --size 8
reserved: 127.0.1.0/24
# region 2
> ip-pool reserve-subnet --state-file state.json --vnet-key region2 --key my-snet1 --size 8
reserved: 127.64.0.0/24
> ip-pool reserve-subnet --state-file state.json --vnet-key region2 --key my-snet2 --size 8
reserved: 127.64.1.0/24
```

**```state.json```**

```json
{
  "addressSpace": "127.0.0.0/8",
  "virtualNetworks": {
    "region1": {
      "addressSpace": "127.0.0.0/10",
      "subnets": {
        "my-snet1": {
          "addressSpace": "127.0.0.0/24"
        },
        "my-snet2": {
          "addressSpace": "127.0.1.0/24"
        }
      }
    },
    "region2": {
      "addressSpace": "127.64.0.0/10",
      "subnets": {
        "my-snet1": {
          "addressSpace": "127.64.0.0/24"
        },
        "my-snet2": {
          "addressSpace": "127.64.1.0/24"
        }
      }
    }
  }
}
```

You can also optionally generate a ```.tf``` file, which makes integration into Terraform a lot easier.

```bash
> ip-pool generate-tf --state-file state.json --variable-name my-ips
```

**```state.tf```**

```terraform
locals {
  my-ips = {
    region1 = {
      address_space = "127.0.0.0/10"
      subnets = {
        my-snet1 = {
          address_space = "127.0.0.0/24"
        }
        my-snet2 = {
          address_space = "127.0.1.0/24"
        }
      }
    }
    region2 = {
      address_space = "127.64.0.0/10"
      subnets = {
        my-snet1 = {
          address_space = "127.64.0.0/24"
        }
        my-snet2 = {
          address_space = "127.64.1.0/24"
        }
      }
    }
  }
}
```
