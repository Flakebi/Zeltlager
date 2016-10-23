using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Serialisation
{
	/// <summary>
	/// An interface that enables you to override automatically generated
	/// serialisation and deserialisation methods.
	/// You must also provide the following static method:
	/// Task<T> ReadFromId(BinaryReader input, Serialiser<C> serialiser, C context)
	/// </summary>
	public interface ISerialisable<C>
	{
		Task Write(BinaryWriter output, Serialiser<C> serialiser, C context);
		Task WriteId(BinaryWriter output, Serialiser<C> serialiser, C context);
		Task Read(BinaryReader input, Serialiser<C> serialiser, C context);
	}
}
