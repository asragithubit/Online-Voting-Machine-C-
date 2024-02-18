using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using Assignment;

namespace Assignment
{
    internal class Voter
    {

        private string voterName;
        private string cnic;
        private string selectedPartyName;

        public Voter()
        {
            voterName = null;
            cnic = null;
            selectedPartyName = null;
        }
        public Voter(string voterName, string cnic, string selectedPartyName)
        {
            this.voterName = voterName;
            this.cnic = cnic;
            this.selectedPartyName = selectedPartyName;
        }

        public string SelectedPartyName
        {
            get
            {
                return selectedPartyName;
            }
        }

        public string CNIC
        {
            get
            {
                return cnic;
            }
        }

        public void setData()
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Insert into Voter(voterName,cnic,selectedPartyName) values(@voterName,@cnic,@selectedPartyName)";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@voterName", voterName);
            command.Parameters.AddWithValue("@cnic", cnic);
            command.Parameters.AddWithValue("@selectedPartyName", selectedPartyName);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public bool hasVoted(string _cnic)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Select cnic,selectedPartyName from Voter";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string c = reader[0].ToString();
                if (c == _cnic)
                {
                    if (reader[1].ToString() == "")
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

//class Program
//{

//    public static void Main(string[] args)
//    {
//        string voterName, cnic, selectedPartyName;
//        Console.WriteLine("Voter Name : ");
//        voterName = Console.ReadLine();
//        Console.WriteLine("CNIC : ");
//        cnic = Console.ReadLine();
//        Console.WriteLine("Selected Party Name : ");
//        selectedPartyName = Console.ReadLine();

//        Voter v = new Voter(voterName, cnic, selectedPartyName);
//        v.setData();
//    }
//}
