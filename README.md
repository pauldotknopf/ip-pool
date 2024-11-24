# ip-cidr-reservation
A CLI tool that helps you manage and reserve IP CIDRs.

Consider the following scenario:

**Pseudo code:**

```
var pool = new Pool("10.0.0.0/10");
var vnet1 = pool.Reserve(8) // bits to reserve, 256 IP addresses
var vnet2 = pool.Reserve(10) // bits to reserve, 1024 IP addresses.
```

For git-ops scenarios, you'd like the reservations stored in a state file.

```bash
ip-cidr-reservation create-pool "10.0.0.0/10" ./pool.json
ip-cidr-reservation reserve "vnet1" "/24" ./pool.json
ip-cidr-reservation reserve "vnet2" "/24" ./pool.json
```

**./pool.json**

```json
{
  "pool": "10.0.0.0/10",
  "reserved": [
    {
      "key": "vnet1",
      "pool": "10.0.0.0/24"
    },
    {
      "key": "vnet2",
      "pool": "10.0.1.0/24"
    }
  ]
}
```

You can check the ```./pool.json``` file into your repository, and other people can request a new range of IPs from the pool, without conflicing with any other already-reserved masks.

We'd have to make sure that the addresses don't become fragmented.

Optionally, we could provide a method to "unfragment" your pool, which would give you a detailed list of what reservations will have to change. I doub't people would ever need this (how big is you fuckin network?!)
