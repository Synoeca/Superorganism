﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public enum Strategy
	{
		Idle = 0,
		AvoidEnemy = 1,
		RandomFlyingMovement = 2,
		Random360FlyingMovement = 3,
		Patrol,
		ChaseEnemy = 5,
		ChargeEnemy = 6,
	}
}
