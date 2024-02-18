using Assignment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.Json;
using System.IO;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;

namespace Assignment
{
    internal class VotingMachine
    {
        private List<Candidate> candidates;
        public VotingMachine()
        {
            candidates = new List<Candidate>();

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = "Select * from Candidate";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Candidate c = new Candidate();
                c.CandidateId = reader.GetInt32(0);
                c.Name = reader.GetString(1);
                c.Party = reader.GetString(2);
                c.Votes = reader.GetInt32(3);
                candidates.Add(c);

            }
        }

        public void castVote(Candidate c, Voter v)
        {
            c.incrementVotes();

            //Updating Votes in Database
            string connStr1 = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConn1 = new SqlConnection(connStr1);
            sqlConn1.Open();
            string qry1 = "Select votes from Candidate where party=@party";
            SqlCommand cmd1 = new SqlCommand(qry1, sqlConn1);
            cmd1.Parameters.AddWithValue("@v", c.Votes);
            cmd1.Parameters.AddWithValue("@party", c.Party);
            int tempVotes =(int)cmd1.ExecuteScalar();

            string connStr2 = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConn2= new SqlConnection(connStr2);
            sqlConn2.Open();
            string qry2 = "Update Candidate Set votes=@v where party=@party";
            SqlCommand cmd2 = new SqlCommand(qry2, sqlConn2);
            cmd2.Parameters.AddWithValue("@v", tempVotes+1);
            cmd2.Parameters.AddWithValue("@party", c.Party);
            cmd2.ExecuteNonQuery();

            //Updating Votes in File 
            FileStream fin1 = new FileStream("Candidate.txt", FileMode.Open);
            StreamReader sr1 = new StreamReader(fin1);
            List<string> strings1 = new List<string>();
            while (true)
            {
                string data = sr1.ReadLine();
                if (data == null)
                {
                    break;
                }
                Candidate c1 = JsonSerializer.Deserialize<Candidate>(data);
                if (c1.Party == c.Party)
                {
                    c.Votes = tempVotes+1;
                    string json = JsonSerializer.Serialize(c);
                    strings1.Add(json);
                }
                else
                {
                    strings1.Add(data);
                }
            }

            // Close the file after reading
            sr1.Close();
            fin1.Close();

            // Truncate the file before writing
            FileStream f1 = new FileStream("Candidate.txt", FileMode.Truncate);
            f1.Close();

            // Open the file for writing
            FileStream f2 = new FileStream("Candidate.txt", FileMode.Open);
            StreamWriter s = new StreamWriter(f2);
            for (int i = 0; i < strings1.Count; i++)
            {
                s.WriteLine(strings1[i]);
            }

            // Close the file after writing
            s.Close();
            f2.Close();
            //Storing the name of party(Candidate) in selectedPartyName of Voter in Database
            string partyName = c.Party;
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Update Voter Set selectedPartyName=@partyName";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@partyName", partyName);
            command.ExecuteNonQuery();
            connection.Close();


            //Storing the name of party(Candidate) in selectedPartyName of Voter in File
            FileStream fin = new FileStream("Voter.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fin);
            List<string> strings = new List<string>();
            while (true)
            {
                string data = sr.ReadLine();
                if (data == null)
                {
                    break;
                }
                string[] arr = data.Split(',');

                if (arr[1] == v.CNIC)
                {
                    string newData = arr[0] + "," + arr[1] + "," + c.Party;
                    strings.Add(newData);
                }
                else
                {
                    strings.Add(data);
                }
            }

            sr.Close();
            fin.Close();

            FileStream f11 = new FileStream("Voter.txt", FileMode.Truncate);
            f11.Close();

            FileStream f21 = new FileStream("Voter.txt", FileMode.Open);
            StreamWriter s1 = new StreamWriter(f21);
            for (int i = 0; i < strings.Count; i++)
            {
                s1.WriteLine(strings[i]);
            }
            s1.Close();
            f21.Close();
        }

        public void addVoter()
    {

        //Storing data in Database
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("1. Add Voter");
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("Enter Voter Details : ");
        Console.Write("Name : ");
        string name = Console.ReadLine();
        Console.Write("CNIC : ");
        string cnic = Console.ReadLine();

        Voter v = new Voter(name, cnic, "");
        v.setData();
        Console.WriteLine("Voter added Successfully!");


        //Storing Data in File
        string data = string.Empty;
        data = name + "," + cnic + "," + v.SelectedPartyName;

        FileStream fin = new FileStream("Voter.txt", FileMode.Append);
        StreamWriter sw = new StreamWriter(fin);
        sw.WriteLine(data);
        sw.Close();
        fin.Close();
    }

    public void updateVoter()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("2. Update Voter");
        Console.WriteLine("----------------------------------------------------------------");
        Console.Write("Enter CNIC of Voter to Update : ");
        string cnic = Console.ReadLine();
        Console.WriteLine("Enter New Voter Details : ");
        Console.Write("Name : ");
        string name = Console.ReadLine();


        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "UPDATE Voter SET voterName = @Name WHERE CNIC = @CNIC";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@CNIC", cnic);
        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
        {
            Console.WriteLine("Voter with Such CNIC doesn't exist!");
        }
        else
        {

            //Updating Data in File
            FileStream fin = new FileStream("Voter.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fin);
            List<string> strings = new List<string>();
            while (true)
            {
                string data = sr.ReadLine();
                if (data == null)
                {
                    break;
                }
                string[] arr = data.Split(',');
                if (arr[1] == cnic)
                {
                    string newData = name + "," + cnic;
                    strings.Add(newData);
                }
                else
                {
                    strings.Add(data);
                }

            }
            sr.Close();
            fin.Close();

            FileStream f1 = new FileStream("Voter.txt", FileMode.Truncate);
            f1.Close();

            FileStream f2 = new FileStream("Voter.txt", FileMode.Open);
            StreamWriter s = new StreamWriter(f2);
            for (int i = 0; i < strings.Count; i++)
            {
                s.WriteLine(strings[i]);
            }
            s.Close();
            f2.Close();
            Console.WriteLine("Voter Updated Successfully!");
        }
    }

    public void displayVoters()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("3. Display Voters");
        Console.WriteLine("----------------------------------------------------------------");

        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        // Query to count the number of voters
        string countQuery = "SELECT COUNT(*) FROM Voter";
        SqlCommand countCommand = new SqlCommand(countQuery, connection);
        int voterCount = (int)countCommand.ExecuteScalar();

        if (voterCount == 0)
        {
            Console.WriteLine("No Voters available!");
        }
        else
        {

            Console.WriteLine("List of Voters : ");
            string query = "Select voterName,CNIC from Voter";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();


            int voterNumber = 1;
            while (reader.Read())
            {
                Console.Write(voterNumber + ". ");
                string name = reader[0].ToString();
                string cnic = reader[1].ToString();
                Console.WriteLine(name + " - CNIC: " + cnic);
                voterNumber++;
            }

            Console.WriteLine("Voters Displayed Successfully!");
        }

    }

    public void displayCandidates()
    {

        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("4. Display Candidates");
        Console.WriteLine("----------------------------------------------------------------");

        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        // Query to count the number of candidates
        string countQuery = "SELECT COUNT(*) FROM Candidate";
        SqlCommand countCommand = new SqlCommand(countQuery, connection);
        int candidateCount = (int)countCommand.ExecuteScalar();

        if (candidateCount == 0)
        {
            Console.WriteLine("No candidates available!");
        }
        else
        {
            Console.WriteLine("List of Candidates : ");
            Console.WriteLine("Id                  Name                                 Party                        Votes");
            string query = "Select * from Candidate";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string party = reader.GetString(2);
                int votes = reader.GetInt32(3);

                Console.WriteLine(id + "          Candidate " + " " + name + "             " + party + "                      " + votes);
            }
            Console.WriteLine("Candidates Displayed Successfully!");
        }



    }

    public void declareWinner()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("5. Declare Winner");
        Console.WriteLine("----------------------------------------------------------------");
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "Select * from Candidate";
        SqlCommand command = new SqlCommand(query, connection);
        SqlDataReader reader = command.ExecuteReader();
        int maxVotes = 0;
        int idOfWinner;
        string nameOfWinner = string.Empty;
        string partyOfWinner = string.Empty;
        int votesOfWinner = 0;
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            string party = reader.GetString(2);
            int votes = reader.GetInt32(3);

            if (maxVotes < votes)
            {
                maxVotes = votes;
                idOfWinner = id;
                nameOfWinner = name;
                partyOfWinner = party;
                votesOfWinner = votes;
            }
        }

        Console.WriteLine("Winner: " + "Candidate " + nameOfWinner + " Party: " + partyOfWinner + " Votes: " + votesOfWinner);
        Console.WriteLine("Winner Specified Successfully!");
    }

    public void insertCandidate(Candidate c)
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("6. Insert Candidate");
        Console.WriteLine("----------------------------------------------------------------");


        //Storing Data in Database
        Console.WriteLine("Enter Candidate Details:");
        Console.Write("Name: ");
        string name = Console.ReadLine();
        string party = string.Empty;
        while (true)
        {
            Console.Write("Party: ");
            party = Console.ReadLine();

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string queryCheckParty = "SELECT COUNT(*) FROM Candidate WHERE Party = @party";
            SqlCommand commandCheckParty = new SqlCommand(queryCheckParty, connection);
            commandCheckParty.Parameters.AddWithValue("@party", party);
            int existingCandidatesWithParty = (int)commandCheckParty.ExecuteScalar();

            if (existingCandidatesWithParty > 0)
            {
                Console.WriteLine("Candidate With Such Party Name already Exists!!!");
                Console.WriteLine("Choose a Different Party Name");
            }
            else
            {
                break;
            }
        }

        c.Name = name;
        c.Party = party;
        c.setData();
        candidates.Add(c);

        //Storing Data in File
        string JsonString = JsonSerializer.Serialize(c);
        FileStream fin = new FileStream("Candidate.txt", FileMode.Append);
        StreamWriter sw = new StreamWriter(fin);
        sw.WriteLine(JsonString);
        sw.Close();
        fin.Close();
        Console.WriteLine("Candidate Inserted Successfully!");

        }

        public void readCandidate(int id)
    {

        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("7. Read Candidate");
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("Reading Candidate Details from Database...");

        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "Select * from Candidate where CandidateId=@id";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int id1 = reader.GetInt32(0);
            string name = reader.GetString(1);
            string party = reader.GetString(2);
            int votes = reader.GetInt32(3);

            Console.WriteLine("Candidate Id: " + id1);
            Console.WriteLine("Name: " + name);
            Console.WriteLine("Party: " + party);
            Console.WriteLine("Votes: " + votes);
        }

        //Reading Data from File
        Console.WriteLine("");
        Console.WriteLine("Reading Candidate Details from File...");
        FileStream fin = new FileStream("Candidate.txt", FileMode.Open);
        StreamReader sr = new StreamReader(fin);
        string json = sr.ReadLine();
        Candidate c = JsonSerializer.Deserialize<Candidate>(json);

        Console.WriteLine("Candidate Id: " + c.CandidateId);
        Console.WriteLine("Name: " + c.Name);
        Console.WriteLine("Party: " + c.Party);
        Console.WriteLine("Votes: " + c.Votes);
        Console.WriteLine("Data Read Successfully!");
    }

    public void updateCandidate()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("8. Update Candidate");
        Console.WriteLine("----------------------------------------------------------------");
        Console.Write("Enter Id of Candidate to Update : ");
        int id = int.Parse(Console.ReadLine());

        Console.WriteLine("Enter new Details of Candidate");
        Console.Write("Name: ");
        string name = Console.ReadLine();
        Console.Write("Party: ");
        string party = Console.ReadLine();
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "Update Candidate Set party=@party,name=@name where candidateId=@id";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@party", party);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@id", id);
        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
        {
            Console.WriteLine("Candidate with Such Id Doesn't Exist!");
        }
        else
        {

            //Updating Candidate in File
            FileStream fin = new FileStream("Candidate.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fin);
            List<string> strings = new List<string>();
            while (true)
            {
                string data = sr.ReadLine();
                if (data == null)
                {
                    break;
                }
                Candidate c = JsonSerializer.Deserialize<Candidate>(data);
                if (c.CandidateId == id)
                {
                    c.Name = name;
                    c.Party = party;
                    string jsonString = JsonSerializer.Serialize(c);
                    strings.Add(jsonString);

                }
                else
                {
                    strings.Add(data);
                }

            }
            sr.Close();
            fin.Close();

            FileStream f1 = new FileStream("Candidate.txt", FileMode.Truncate);
            f1.Close();

            FileStream f2 = new FileStream("Candidate.txt", FileMode.Open);
            StreamWriter s = new StreamWriter(f2);
            for (int i = 0; i < strings.Count; i++)
            {
                s.WriteLine(strings[i]);
            }
            s.Close();
            f2.Close();
            Console.WriteLine("Candidate Updated Successfully!");
        }
    }

    public void deleteCandidate()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("9. Delete Candidate");
        Console.WriteLine("----------------------------------------------------------------");
        Console.Write("Enter Id of Candidate to Delete : ");
        int id = int.Parse(Console.ReadLine());
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "Delete from candidate where candidateId=@id";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
        {
            Console.WriteLine("Candiate with Such Id Doesn't Exist!");
        }
        else
        {
            //Deleting Candidate from File
            List<string> strings = new List<string>();
            FileStream fin = new FileStream("Candidate.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fin);
            while (true)
            {
                string data = sr.ReadLine();
                if (data == null)
                {
                    break;
                }
                Candidate c = JsonSerializer.Deserialize<Candidate>(data);
                if (c.CandidateId == id)
                {
                    continue;
                }
                strings.Add(data);
            }

            sr.Close();
            fin.Close();

            FileStream f1 = new FileStream("Candidate.txt", FileMode.Truncate);
            f1.Close();

            FileStream f2 = new FileStream("Candidate.txt", FileMode.Open);
            StreamWriter s = new StreamWriter(f2);
            for (int i = 0; i < strings.Count; i++)
            {
                s.WriteLine(strings[i]);
            }
            s.Close();
            f2.Close();
            Console.WriteLine("Candidate Deleted Successfully!");
        }
    }

    public void deleteVoter()
    {
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("10. Delete Voter");
        Console.WriteLine("----------------------------------------------------------------");
        Console.Write("Enter CNIC of Voter to Delete : ");
        string cnic = Console.ReadLine();
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Voter;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        string query = "Delete from Voter where CNIC=@cnic";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@cnic", cnic);
        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
        {
            Console.WriteLine("Voter with Such CNIC Doesn't Exist!");
        }
        else
        {
                FileStream fin = new FileStream("Voter.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fin);
                List<string> strings = new List<string>();
                while (true)
                {
                    string data = sr.ReadLine();
                    if (data == null)
                    {
                        break;
                    }
                    string[] arr = data.Split(',');
                    if (arr[1] == cnic)
                    {
                        continue;
                    }
                    strings.Add(data);

                }
                sr.Close();
                fin.Close();

                FileStream f1 = new FileStream("Voter.txt", FileMode.Truncate);
                f1.Close();

                FileStream f2 = new FileStream("Voter.txt", FileMode.Open);
                StreamWriter s = new StreamWriter(f2);
                for (int i = 0; i < strings.Count; i++)
                {
                    s.WriteLine(strings[i]);
                }
                s.Close();
                f2.Close();
                Console.WriteLine("Voter Deleted Successfully!");
        }
    }
}
}




class Program
{
    public static void Main(string[] args)
    {
        int choice;
        VotingMachine vm = new VotingMachine();
        Console.WriteLine("----------------------------------Welcome to Online Voting System----------------------------------");
        Console.WriteLine("                                  --------------------------------                              ");

        do
        {
            Console.WriteLine("\n1.  Add Voter");
            Console.WriteLine("2.  Update Voter");
            Console.WriteLine("3.  Delete Voter");
            Console.WriteLine("4.  Display Voters");
            Console.WriteLine("5.  Cast Vote");
            Console.WriteLine("6.  Insert Candidate");
            Console.WriteLine("7.  Update Candidate");
            Console.WriteLine("8.  Display Candidate");
            Console.WriteLine("9.  Delete Candidate");
            Console.WriteLine("10. Declare Winner");
            Console.WriteLine("11. Press -1 to Quit");

            Console.WriteLine("");
            Console.WriteLine("");
            Console.Write("Enter Your Choice from 1 to 10 : ");
            while (true)
            {
                choice = int.Parse(Console.ReadLine());
                if (choice >= 1 && choice <= 10 || choice == -1)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Choice!!!");
                    Console.Write("Enter Your Choice from 1 to 10 : ");
                }
            }

            if (choice == -1)
            {
                break;
            }
            if (choice == 1)
            {
                vm.addVoter();
            }
            else if (choice == 2)
            {
                vm.updateVoter();
            }
            else if (choice == 3)
            {
                vm.deleteVoter();
            }
            else if (choice == 4)
            {
                vm.displayVoters();
            }
            else if (choice == 5)
            {
                string name = string.Empty;
                string party = string.Empty;
                string voterName = string.Empty;
                string cnic = string.Empty;

                Console.Write("Name of Candidate: ");
                name = Console.ReadLine();
                Console.Write("Party Name: ");
                party = Console.ReadLine();

                Console.Write("Voter Name: ");
                voterName = Console.ReadLine();
                Console.Write("CNIC: ");
                cnic = Console.ReadLine();
                bool flag = false;
                Candidate c=new Candidate();

                FileStream fin = new FileStream("Candidate.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fin);
                while (true)
                {
                    string data = sr.ReadLine();
                    if (data == null)
                    {
                        break;
                    }
                    c = JsonSerializer.Deserialize<Candidate>(data);

                    if (c.Name == name)
                    {
                        flag = true;
                        break;
                    }
                }

                sr.Close();
                fin.Close();

                if (flag == false)
                {
                    Candidate c1 = new Candidate(name, party);
                    Voter v = new Voter(voterName, cnic, "");
                    vm.castVote(c1, v);
                }
                else
                {
                    Voter v = new Voter(voterName, cnic, "");
                    vm.castVote(c, v);
                }
            }
            else if (choice == 6)
            {
                Candidate c = new Candidate("", "");
                vm.insertCandidate(c);
            }
            else if (choice == 7)
            {
                Candidate c = new Candidate("", "");
                vm.updateCandidate();
            }
            else if (choice == 8)
            {
                vm.displayCandidates();
            }
            else if (choice == 9)
            {
                vm.deleteCandidate();
            }
            else
            {
                vm.declareWinner();
            }
        } while (choice != -1);
    }
}
