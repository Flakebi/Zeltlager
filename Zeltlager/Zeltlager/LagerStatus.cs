using System;
using System.Collections.Generic;

namespace Zeltlager
{
	using Cryptography;

	/// <summary>
	/// Stores the status of a lager: How many packets were created by each collaborator.
	/// </summary>
	public class LagerStatus
	{
		readonly List<Tuple<KeyPair, int>> bundleCount = new List<Tuple<KeyPair, int>>();
		
		/// <summary>
		/// The collaborator list saves the bundle count for each collaborator.
		/// This is the next bundle id that can be uploaded to the server.
		/// The index in this list is the collaborator id as seen from the owner of this object.
		/// </summary>
		public IReadOnlyList<Tuple<KeyPair, int>> BundleCount => bundleCount;

		/// <summary>
		/// Get the collaborator id from the view of the owner of this object.
		/// </summary>
		/// <returns>The collaborator id.</returns>
		/// <param name="c">The collaborator whos id will be returned.</param>
		public int GetCollaboratorId(Collaborator c)
		{
			return bundleCount.FindIndex(t => t != null && t.Item1 == c.Key);
		}

		/// <summary>
		/// Get the amount of bundles of a collaborator.
		/// </summary>
		/// <returns>How many bundles the given collaborator created.</returns>
		/// <param name="c">The creator of the bundles.</param>
		public int GetBundleCount(Collaborator c)
		{
			var col = bundleCount.Find(t => t.Item1 == c.Key);
			if (col == null)
				return 0;
			else
				return col.Item2;
		}

		/// <summary>
		/// Sets the bundle count of a collaborator.
		/// </summary>
		/// <param name="c">The collaborator of which the bundle count should be set.</param>
		/// <param name="count">The new amount of bundles.</param>
		public void SetBundleCount(Collaborator c, int count)
		{
			int index = bundleCount.FindIndex(t => t.Item1 == c.Key);
			bundleCount[index] = new Tuple<KeyPair, int>(c.Key, count);
		}

		/// <summary>
		/// Set the bundle count of a collaborator to the currently available amount of bundles.
		/// </summary>
		/// <param name="c">The collaborater of which the bundle count should be updated.</param>
		public void UpdateBundleCount(Collaborator c)
		{
			SetBundleCount(c, c.Bundles.Count);
		}

		public void AddBundleCount(Tuple<KeyPair, int> count)
		{
			bundleCount.Add(count);
		}
	}
}
