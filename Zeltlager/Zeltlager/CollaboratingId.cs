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
		protected CollaboratingId(LagerClient lager, BinaryReader input)
		{
			Read(lager, input);
		}

		public bool Equals(CollaboratingId<T> other)
		{
			return collaborator.Id == other.collaborator.Id && id.Equals(other.id);
		}

		public void Read(LagerClient lager, BinaryReader input)
		{
			byte collaboratorId = input.ReadByte();
			collaborator = lager.Collaborators.First(c => c.Id == collaboratorId);
			id = ReadId(lager, input);
		}

		protected abstract T ReadId(LagerClient lager, BinaryReader input);

		public void Write(BinaryWriter output)
		{
			output.Write(collaborator.Id);
			WriteId(output);
		}

		protected abstract void WriteId(BinaryWriter output);
	}

	public class TentId : CollaboratingId<byte>
	{
		public TentId() { }
		public TentId(LagerClient lager, BinaryReader input) : base(lager, input) { }

		protected override byte ReadId(LagerClient lager, BinaryReader input)
		{
			return input.ReadByte();
		}

		protected override void WriteId(BinaryWriter output)
		{
			output.Write(id);
		}
	}

	public class MemberId : CollaboratingId<ushort>
	{
		public MemberId() { }
		public MemberId(LagerClient lager, BinaryReader input) : base(lager, input) { }

		protected override ushort ReadId(LagerClient lager, BinaryReader input)
		{
			return input.ReadUInt16();
		}

		protected override void WriteId(BinaryWriter output)
		{
			output.Write(id);
		}
	}
}
