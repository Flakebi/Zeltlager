using System;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	using DataPackets;
	using Newtonsoft.Json;
	
	/// <summary>
	/// A participant for the Erwischt game.
	/// </summary>
	public class ErwischtParticipant : ISearchable, IComparable<ErwischtParticipant>
	{
		/// <summary>
		/// The game this ErwischtMember belongs to.
		/// </summary>
		public ErwischtGame Game { get; set; }

		/// <summary>
		/// The member this ErwischtMember is representing.
		/// </summary>
		// todo json ref
		public Member Member { get; private set; }

		/// <summary>
		/// The index of this participant in the list of the game.
		/// </summary>
		int index;

		/// <summary>
		/// The first target of this participant, this is not neccessarily the
		/// next target that is alive.
		/// </summary>
		ErwischtParticipant target => Game.ErwischtParticipants[
			(index + 1) % Game.ErwischtParticipants.Count];

		/// <summary>
		/// The participant before this one in the list.
		/// </summary>
		ErwischtParticipant catcher => Game.ErwischtParticipants[
			(index + Game.ErwischtParticipants.Count - 1) % Game.ErwischtParticipants.Count];

		/// <summary>
		/// The participant that this participant needs to catch next.
		/// </summary>

		[JsonIgnore] 
		public ErwischtParticipant Target
		{
			get
			{
				ErwischtParticipant target = this.target;
				while (!target.IsAlive && target != this)
				{
					target = target.target;
				}
				return target;
			}
		}

		/// <summary>
		/// The participant that needs to catch this participant next.
		/// </summary>

		[JsonIgnore] 
		public ErwischtParticipant Catcher
		{
			get
			{
				ErwischtParticipant catcher = this.catcher;
				while (!catcher.IsAlive && catcher != this)
				{
					catcher = catcher.catcher;
				}
				return catcher;
			}
		}

		bool isAlive = true;

		/// <summary>
		/// Indicating whether this <see cref="T:Zeltlager.Erwischt.ErwischtMember"/> is still in the game.
		/// </summary>
		/// <value><c>true</c> if it is alive; if it was catched, <c>false</c>.</value>

		public bool IsAlive
		{
			get
			{
				return isAlive;
			}

			set
			{
				if (isAlive != value)
				{
					if (value)
					{
						// This participant gets revived so decrease the catched
						// counter of our catcher
						if (lastCatcher != null)
						{
							lastCatcher.Catches--;
							lastCatcher = null;
						}
					}
					else
					{
						lastCatcher = Catcher;
						lastCatcher.Catches++;
					}
					isAlive = value;
				}
			}
		}

		/// <summary>
		/// The amount of members that where catched by this participant.
		/// </summary>

		[JsonIgnore] 
		public int Catches { get; set; }

		/// <summary>
		/// The participant that catched the current participant most recently.
		/// This is used if a participant is revived so the Catches count of the
		/// catcher can be reduced.
		/// </summary>
		ErwischtParticipant lastCatcher;


		[JsonIgnore] 
		public string SearchableText => Member.Display;

		[JsonIgnore] 
		public string SearchableDetail
		{
			get 
			{
				if (!IsAlive)
					return "erwischt!";
				if (this == Target)
					return "Gewinner \ud83c\udf89";
				if (Game.GetLager().ClientManager.Settings.ErwischtShowTarget)
					return "→ " + Target.Member.Display;
				return "";
			}
		}

		public ErwischtParticipant() { }

		public ErwischtParticipant(Member member, int index, ErwischtGame game)
		{
			Game = game;
			Member = member;
			this.index = index;
		}

		public void SetIndex(int index)
		{
			this.index = index;
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
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Game.GetLager());
			Serialiser<LagerClientSerialisationContext> serialiser = Game.GetLager().ClientSerialiser;
			DataPacket packet = await ErwischtPacket.Create(serialiser, context, this, isAlive);
			await context.LagerClient.AddPacket(packet);
		}

		public int CompareTo(ErwischtParticipant other)
		{
			if (other.IsAlive && !this.IsAlive)
			{
				return 1;
			}
			if (!other.IsAlive && this.IsAlive)
			{
				return -1;
			}
			return this.Member.CompareTo(other.Member);
		}

		public override string ToString()
		{
			return "ErwischtParticipant " + Member.Name + " " + (IsAlive ? "alive" : "catched") + " -> " + SearchableDetail;
		}
	}
}
