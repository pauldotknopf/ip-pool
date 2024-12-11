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
