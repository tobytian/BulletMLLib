﻿
namespace BulletMLLib
{
	// このインタフェースを実装し、BulletMLManager.Init()を初期化のために必ず呼ぶこと。
	public interface IBulletMLManager
	{

		float GetRandom();

		float GetRank();

		float GetShipPosX();

		float GetShipPosY();
	}
}