﻿using System;

namespace BulletMLLib
{
	/// <summary>
	/// BulletML Fire処理
	/// </summary>
	internal class BulletMLFire : BulletMLTask
	{
		#region Members

		BulletMLNode refNode, dirNode, spdNode, bulletNode;

		#endregion //Members

		#region Methods

		public BulletMLFire(BulletMLNode node, BulletMLTask owner) : base(node, owner)
		{
			this.dirNode = node.GetChild(ENodeName.direction);
			this.spdNode = node.GetChild(ENodeName.speed);
			this.refNode = node.GetChild(ENodeName.bulletRef);
			this.bulletNode = node.GetChild(ENodeName.bullet);

			if (dirNode == null && refNode != null)
			{
				dirNode = refNode.GetChild(ENodeName.direction);
			}

			if (dirNode == null && bulletNode != null)
			{
				dirNode = bulletNode.GetChild(ENodeName.direction);
			}

			if (spdNode == null && refNode != null)
			{
				spdNode = refNode.GetChild(ENodeName.speed);
			}

			if (spdNode == null && bulletNode != null)
			{
				spdNode = bulletNode.GetChild(ENodeName.speed);
			}
		}

		public override ERunStatus Run(Bullet bullet)
		{
			float changeDir = 0;
			float changeSpd = 0;

			// 方向の設定
			if (dirNode != null)
			{
				changeDir = (int)dirNode.GetValue(this) * (float)Math.PI / (float)180;
				if (dirNode.NodeType == ENodeType.sequence)
				{
					bullet.GetFireData().srcDir += changeDir;
				}
				else if (dirNode.NodeType == ENodeType.absolute)
				{
					bullet.GetFireData().srcDir = changeDir;
				}
				else if (dirNode.NodeType == ENodeType.relative)
				{
					bullet.GetFireData().srcDir = changeDir + bullet.Direction;
				}
				else
				{
					bullet.GetFireData().srcDir = changeDir + bullet.GetAimDir();
				}
			}
			else
			{
				bullet.GetFireData().srcDir = bullet.GetAimDir();
			}

			// 弾の生成
			Bullet newBullet = bullet.MyBulletManager.CreateBullet();//bullet.tree);

			if (newBullet == null)
			{
				TaskFinished = true;
				return ERunStatus.End;
			}

			if (refNode != null)
			{
				// パラメータを取得
				for (int i = 0; i < refNode.ChildNodes.Count; i++)
				{
					newBullet._tasks[0].ParamList.Add(refNode.ChildNodes[i].GetValue(this));
				}

				newBullet.Init(bullet._myNode.FindLabelNode(refNode.Label, ENodeName.bullet));
			}
			else
			{
				newBullet.Init(bulletNode);
			}

			newBullet.X = bullet.X;
			newBullet.Y = bullet.Y;
			newBullet._tasks[0].Owner = this;
			newBullet.Direction = bullet.GetFireData().srcDir;

			if (!bullet.GetFireData().speedInit && newBullet.GetFireData().speedInit)
			{
				// 自分の弾発射速度の初期化がまだのとき、子供の弾の速度を使って初期値とする
				bullet.GetFireData().srcSpeed = newBullet.Velocity;
				bullet.GetFireData().speedInit = true;
			}
			else
			{
				// 自分の弾発射速度の初期化済みのとき
				// スピードの設定
				if (spdNode != null)
				{
					changeSpd = spdNode.GetValue(this);
					if (spdNode.NodeType == ENodeType.sequence || spdNode.NodeType == ENodeType.relative)
					{
						bullet.GetFireData().srcSpeed += changeSpd;
					}
					else
					{
						bullet.GetFireData().srcSpeed = changeSpd;
					}
				}
				else
				{
					// 特に弾に速度が設定されていないとき
					if (!newBullet.GetFireData().speedInit)
					{
						bullet.GetFireData().srcSpeed = 1;
					}
					else
					{
						bullet.GetFireData().srcSpeed = newBullet.Velocity;
					}
				}
			}

			newBullet.GetFireData().speedInit = false;
			newBullet.Velocity = bullet.GetFireData().srcSpeed;

			TaskFinished = true;
			return ERunStatus.End;
		}

		#endregion //Methods
	}
}