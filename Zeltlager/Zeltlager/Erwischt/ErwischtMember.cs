using System;
namespace Zeltlager.Erwischt
{
	/// <summary>
	/// A participant for the Erwischt game.
	/// </summary>
	public class ErwischtMember : IDeletable, ISearchable
	{
		/// <summary>
		/// The game this ErwischtMember belongs to.
		/// </summary>
		Erwischt game;

		/// <summary>
		/// The member this ErwischtMember is representing.
		/// </summary>
		public Member Member { get; private set; }

		/// <summary>
		/// The first target of the ErwischtMember. Should not be changed if it gets catched!
		/// </summary>
		ErwischtMember target;

		public ErwischtMember Target
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
		public bool IsAlive { get; set; }

		/// <summary>
		/// Indicating whether this <see cref="T:Zeltlager.Erwischt.ErwischtMember"/> is visible or was deleted.
		/// </summary>
		/// <value><c>true</c> if still there; if deleted, <c>false</c>.</value>
		public bool IsVisible { get; set; }

		public string SearchableText => Member.Name;
		public string SearchableDetail => "-> " + Target;

		public ErwischtMember(Member member, ErwischtMember target, Erwischt game)
		{
			this.game = game;
			Member = member;
			this.target = target;
		}

		public void SetInitialTarget(ErwischtMember target)
		{
			this.target = target;
		}
	}
}
