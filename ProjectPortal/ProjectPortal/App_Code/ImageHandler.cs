using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.SessionState;

namespace EventScheduler
{
	public class ImageHandler : IHttpHandler, IRequiresSessionState
	{
		#region IHttpHandler Members

		private DataServer _dataServer;

		private DataServer DataServer
		{
			get
			{
				if (_dataServer == null)
				{
					string connectionString =
						ConfigurationManager.ConnectionStrings["RadSchedulerConnectionString"].ConnectionString;

					_dataServer = new DataServer(connectionString);
				}

				return _dataServer;
			}
		}

		private HttpContext Context { get; set; }

		public void ProcessRequest(HttpContext context)
		{
			Context = context;

			string imageFilename = context.Request.QueryString["imageFilename"];
			string extension = String.Empty;

			if (imageFilename == null)
			{
				return;
			}

			byte[] bytes = null;
			if (context.Session != null && context.Session["bytes"] != null)
			{
				bytes = (byte[])context.Session["bytes"];
			}
			else
			{
				bytes = DataServer.GetContent(imageFilename);
			}
			if (bytes == null)
			{
				return;
			}
			if (context.Session["extension"] != null)
			{
				extension = context.Session["extension"].ToString();
			}
			else
			{
				extension = DataServer.GetExtension(imageFilename);
			}
			WriteFile(bytes, imageFilename, extension, context.Response);

		}

		/// <summary>
		/// Sends a byte array to the client
		/// </summary>
		/// <param name="content">binary file content</param>
		/// <param name="fileName">the filename to be sent to the client</param>
		/// <param name="contentType">the file content type</param>
		private void WriteFile(byte[] content, string filename, string extension, HttpResponse response)
		{
			response.Buffer = true;
			response.Clear();
			response.ContentType = "image/" + extension.Replace(".", "");

			response.AddHeader("Content-disposition", "attachment; filename=" + filename + extension);

			response.BinaryWrite(content);
			response.Flush();
			response.End();
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}