using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.IO;


class Candidate
{
    private static int nextCandidateId = 1;
    private int candidateId;
    private string name;
    private string party;
    private int votes;

    private int GenerateCandidateId()
    {
        Random random = new Random();
        int id;


        while (true)
        {
            id = random.Next(1, 1001);
            bool flag = true;
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = "Select candidateId from Candidate";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (id == reader.GetInt32(0))
                {
                    flag = false;
                    break;
                }
            }

            if (flag == true)
            {
                break;
            }
        }
        return id;
    }

    public Candidate()
    {
        candidateId = 0;
        name = null;
        party = null;
        votes = 0;
    }
    public Candidate(string name, string party)
    {
        this.name = name;
        this.party = party;
        candidateId = GenerateCandidateId();
        votes = 0;
    }


    //Function to Set Values in Database
    public void setData()
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = $"Insert into Candidate(candidateId,name,party,votes) values('{candidateId}','{name}','{party}','{votes}')";
        SqlCommand command = new SqlCommand(query, connection);
        command.ExecuteNonQuery();
    }

    public int CandidateId
    {
        get
        {
            return candidateId;
        }
        set
        {
            candidateId = value;
        }
    }

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public string Party
    {
        get
        {
            return party;
        }
        set
        {
            party = value;
        }
    }

    public int Votes
    {
        get
        {
            return votes;
        }
        set
        {
            votes = value;
        }
    }

    public void incrementVotes()
    {
        votes++;
    }

    public override string ToString()
    {
        string data = candidateId + " " + name + " " + " " + party + " " + votes;
        return data;
    }
}
namespace Assignment
{
    internal class Program
    {
        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Candidate Name : ");
        //    string name = Console.ReadLine();
        //    Console.WriteLine("Candidate Party : ");
        //    string party = Console.ReadLine();
        //    global::Candidate c = new global::Candidate(name, party);
        //    c.setData();
        //    //c.readData();
        //}
    }
}
