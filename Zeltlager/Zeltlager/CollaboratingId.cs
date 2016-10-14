using System;
using System.IO;
using System.Linq;

namespace Zeltlager
{
	using Client;

	public abstract class CollaboratingId<T> : IEquatable<CollaboratingId<T>> where T : IEquatable<T>
	{
		public Collaborator collaborator;
		public T id;

		protected CollaboratingId() { }
		protected CollaboratingId(Collaborator collaborator, T id)
		{
            this.collaborator = collaborator;
            this.id = id;
		}

		public bool Equals(CollaboratingId<T> other)
		{
			if (other == null)
				return false;
			return collaborator.Id == other.collaborator.Id && id.Equals(other.id);
		}

		public override bool Equals(object obj)
		{
			CollaboratingId<T> other = obj as CollaboratingId<T>;
			if (other == null)
				return false;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode() ^ collaborator.Id.GetHashCode();
		}

		public abstract void WriteId(BinaryWriter output);

		public static bool operator ==(CollaboratingId<T> c1, CollaboratingId<T> c2)
		{
			if ((object)c1 == null)
				return (object)c2 == null;
			return c1.Equals(c2);
		}

		public static bool operator !=(CollaboratingId<T> c1, CollaboratingId<T> c2)
		{
			return !(c1 == c2);
		}
	}

	public class TentId : CollaboratingId<byte>
	{
		public TentId() { }
		public TentId(Collaborator collaborator, byte id) : base(collaborator, id) { }

        public override void WriteId(BinaryWriter output)
		{
			output.Write(id);
		}
	}

	public class MemberId : CollaboratingId<ushort>
	{
		public MemberId() { }
		public MemberId(Collaborator collaborator, ushort id) : base(collaborator, id) { }

        public override void WriteId(BinaryWriter output)
		{
			output.Write(id);
		}
	}

    // Extensions for BinaryWriter/Reader
    public static class CollaboratingHelper
    {
        public static void Write<T>(this BinaryWriter output, CollaboratingId<T> id) where T : IEquatable<T>
        {
            output.Write(id.collaborator.Id);
            id.WriteId(output);
        }

        public static TentId ReadTentId(this BinaryReader input, LagerClient lager)
        {
            byte collaboratorId = input.ReadByte();
            var collaborator = lager.Collaborators.First(c => c.Id == collaboratorId);
            byte tentId = input.ReadByte();
            return new TentId(collaborator, tentId);
        }

        public static MemberId ReadMemberId(this BinaryReader input, LagerClient lager)
        {
            byte collaboratorId = input.ReadByte();
            var collaborator = lager.Collaborators.First(c => c.Id == collaboratorId);
            byte tentId = input.ReadByte();
            return new MemberId(collaborator, tentId);
        }
    }
}
