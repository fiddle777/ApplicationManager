using System;

// REIKALAVIMAS: Panaudotas savas išimties tipas (1 t.)
// ApplicationRepositoryException metama repo sluoksnyje ir pagaunama Program.cs.

namespace ApplicationManager.Core.Exceptions
{
	public class ApplicationRepositoryException : Exception
	{
		public ApplicationRepositoryException(string message) : base(message) { }
		public ApplicationRepositoryException(string message, Exception inner) : base(message, inner) { }
	}
}
