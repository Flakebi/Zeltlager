using System.IO;

namespace Zeltlager.Serialisation
{
	/// <summary>
	/// An interface that enables you to override automatically generated
	/// serialisation and deserialisation methods.
	/// You must also provide the following static methods:
	/// Read(BinaryReader input, SerialisationContext context)
	/// ReadFromId(BinaryReader input, SerialisationContext context)
	/// </summary>
	public interface ISerialisable
	{
		void Write(BinaryWriter output, SerialisationContext context);
		void WriteId(BinaryWriter output, SerialisationContext context);
	}
}
