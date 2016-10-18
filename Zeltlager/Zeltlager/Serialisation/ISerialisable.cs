using System.IO;

namespace Zeltlager.Serialisation
{
	/// <summary>
	/// An interface that enables you to override automatically generated
	/// serialisation and deserialisation methods.
	/// You must also provide the following static method:
	/// ReadFromId(BinaryReader input, Serialiser<C> serialiser, C context)
	/// </summary>
	public interface ISerialisable<C>
	{
		void Write(BinaryWriter output, Serialiser<C> serialiser, C context);
		void WriteId(BinaryWriter output, Serialiser<C> serialiser, C context);
		void Read(BinaryReader input, Serialiser<C> serialiser, C context);
	}
}
