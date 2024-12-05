using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism
{
	public enum Strategy
	{
		Idle = 0,
		Transition = 1,
		AvoidEnemy = 2,
		RandomFlyingMovement = 3,
		Random360FlyingMovement = 4,
		Patrol = 5,
		ChaseEnemy = 6,
		ChargeEnemy = 7,
	}
}
