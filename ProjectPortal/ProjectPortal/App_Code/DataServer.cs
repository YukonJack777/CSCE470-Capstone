using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace EventScheduler
{
	/// <summary>
	/// Retrieves data from the database and serves content to the DBContentProvider
	/// </summary>
	public class DataServer
	{
		private string _connectionString;

		/// <summary>
		/// Creates an instance of the DataServer class
		/// </summary>
		/// <param name="connectionString">The connection string used to establish a connection with the database</param>
		public DataServer(string connectionString)
		{
			_connectionString = connectionString;
		}

		private delegate T DbCallback<T>(SqlCommand command);

		/// <summary>
		/// Helper function for database queries.
		/// Accepts text-based SQL command and array of parameters, and returns the return value of the callback.
		/// </summary>
		/// <typeparam name="T">The return value type</typeparam>
		/// <param name="commandText">SQL command</param>
		/// <param name="parameters">Array of named parameters for database query</param>
		/// <param name="callback">Callback that should be called when connection is in place</param>
		/// <returns>The return value of the callback</returns>
		private T ExecuteDbQuery<T>(string commandText, SqlParameter[] parameters, DbCallback<T> callback)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = new SqlCommand(commandText, connection))
				{
					connection.Open();

					if (parameters != null)
						command.Parameters.AddRange(parameters);

					return callback(command);
				}
			}
		}

		public byte[] GetContent(string imageName)
		{
			return ExecuteDbQuery(
				@"SELECT Content FROM [Profile] WHERE ImageName = @ImageName",
				new SqlParameter[] { new SqlParameter("@ImageName", imageName) },
				command => (byte[])command.ExecuteScalar());
		}

		public bool HasRecord()
		{
			return ExecuteDbQuery(
				@"SELECT TOP 1 * FROM [Profile]",
				null,
				command => command.ExecuteReader().HasRows);
		}

		public string GetExtension(string imageName)
		{
			return ExecuteDbQuery(
				@"SELECT Extension FROM [Profile] WHERE ImageName = @ImageName",
				new SqlParameter[] { new SqlParameter("@ImageName", imageName) },
				command => command.ExecuteScalar().ToString());
		}

		public string GetImageName()
		{
			return ExecuteDbQuery(
				@"SELECT TOP 1 ImageName FROM [Profile]",
				null,
				command => command.ExecuteScalar().ToString());
		}

		public void Insert(int id)
		{
			ExecuteDbQuery(
				@"INSERT INTO MyAppointments VALUES (@appointmentID)",
				new SqlParameter[] { new SqlParameter("@appointmentID", id) },
				command => command.ExecuteNonQuery());
		}

		public void UpdateProfile(string firstName, string lastName, string company, string position)
		{
			string commandText = "INSERT INTO Profile (FirstName, LastName, Company, Position) VALUES (@firstName, @lastName, @company, @position)";

			if (HasRecord())
				commandText = "UPDATE Profile SET FirstName = @firstName, LastName = @lastName, Company = @company, Position = @position";

			ExecuteDbQuery(
				commandText,
				new SqlParameter[] { 
					new SqlParameter("@firstName", firstName),
					new SqlParameter("@lastName", lastName),
					new SqlParameter("@company", company),
					new SqlParameter("@position", position)
				},
				command => command.ExecuteNonQuery());
		}

		public ProfileData GetProfile()
		{
			return ExecuteDbQuery(
				@"SELECT TOP 1 FirstName, LastName, Company, Position FROM [Profile]",
				null,
				delegate(SqlCommand command)
				{
					SqlDataReader reader = command.ExecuteReader();
					if (reader.Read())
					{
						return new ProfileData()
						{
							FirstName = reader["FirstName"].ToString(),
							LastName = reader["LastName"].ToString(),
							Company = reader["Company"].ToString(),
							Position = reader["Position"].ToString()
						};
					}
					return null;
				});
		}

		public EventData GetEventDetails(string eventID)
		{
			return ExecuteDbQuery(
				@"SELECT * FROM [Appointments] 
                             INNER JOIN [Tracks]
                             ON [Tracks].ID = [Appointments].TrackID 
                             WHERE [Appointments].ID = @ID",
				new SqlParameter[] { new SqlParameter("@ID", eventID) },
				delegate(SqlCommand command)
				{
					SqlDataReader reader = command.ExecuteReader();

					if (reader.Read())
					{
						return new EventData()
						{
							Subject = reader["Subject"].ToString(),
							Track = reader["Track"].ToString(),
							Level = reader["Level"].ToString(),
							Start = (DateTime)reader["Start"],
							End = (DateTime)reader["End"],
							Annotations = reader["Annotations"].ToString()
						};
					}

					return null;
				});
		}

		public void SaveImage(byte[] btContent, string strImageName, string strExtension)
		{
			string commandText = "INSERT INTO Profile ([Content], [ImageName], [Extension]) VALUES (@content, @imageName, @extension)";

			if (HasRecord())
				commandText = "UPDATE Profile SET Content = @content, ImageName = @imageName, Extension = @extension";

			ExecuteDbQuery(
				commandText,
				new SqlParameter[] {
					new SqlParameter("@content", btContent),
					new SqlParameter("@imageName", strImageName),
					new SqlParameter("@extension", strExtension)
				},
				command => command.ExecuteNonQuery());
		}

		public void SetMarked(bool value, object appointmentID)
		{
			ExecuteDbQuery(
				@"UPDATE Appointments SET Marked = @mark  WHERE ID = @appointmentID",
				new SqlParameter[] {
					new SqlParameter("@mark", value),
					new SqlParameter("@appointmentID", Convert.ToInt32(appointmentID))
				},
				command => command.ExecuteNonQuery());
		}
	}
}