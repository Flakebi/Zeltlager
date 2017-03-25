﻿using System;
using Zeltlager.Serialisation;
using Zeltlager.DataPackets;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	/// <summary>
	/// A participant for the Erwischt game.
	/// </summary>
	public class ErwischtParticipant : ISearchable
	{
		/// <summary>
		/// The game this ErwischtMember belongs to.
		/// </summary>
		public ErwischtGame Game { get; set; }

		/// <summary>
		/// The member this ErwischtMember is representing.
		/// </summary>
		[Serialisation(Type = SerialisationType.Reference)]
		public Member Member { get; private set; }

		/// <summary>
		/// The first target of the ErwischtMember. Should not be changed if it gets catched!
		/// </summary>
		ErwischtParticipant target;

		public ErwischtParticipant Target
		{
			get
			{
				if (target.IsAlive)
					return target;
				return target.Target;
			}
		}

		/// <summary>
		/// Indicating whether this <see cref="T:Zeltlager.Erwischt.ErwischtMember"/> is still in the game.
		/// </summary>
		/// <value><c>true</c> if it is alive; if it was catched, <c>false</c>.</value>
		[Serialisation]
		public bool IsAlive { get; set; }

		public string SearchableText => Member.Name;
		public string SearchableDetail => "-> " + Target;

		public ErwischtParticipant() { }

		public ErwischtParticipant(Member member, ErwischtParticipant target, ErwischtGame game)
		{
			Game = game;

			Member = member;
			this.target = target;
		}

		public void SetInitialTarget(ErwischtParticipant target)
		{
			this.target = target;
		}

		public async Task Catch()
		{
			await CreateErwischtPackage(false);
		}

		public async Task Revive()
		{
			await CreateErwischtPackage(true);
		}

		async Task CreateErwischtPackage(bool isAlive)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Game.GetLager().Manager, Game.GetLager());
			Serialiser<LagerClientSerialisationContext> serialiser = Game.GetLager().ClientSerialiser;
			DataPacket packet = await ErwischtPacket.Create(serialiser, context, this, isAlive);
			await context.LagerClient.AddPacket(packet);
		}
	}
}