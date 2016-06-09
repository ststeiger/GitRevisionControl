// using System;
using NGit;
using NGit.Api;
// using NGit.Transport;
using NGit.Revwalk;

using NGit.Treewalk;
using NGit.Treewalk.Filter;

using NGit.Diff;

namespace TestGit
{



    // http://www.mcnearney.net/blog/ngit-tutorial/
    // http://www.codeaffine.com/2014/09/22/access-git-repository-with-jgit/
    class Test
    {


        public static string ReadFile(ObjectLoader loader)
        {
            string strModifiedFile = null;

            using (System.IO.Stream strm = loader.OpenStream())
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(strm))
                {
                    strModifiedFile = sr.ReadToEnd();
                }
            }

            return strModifiedFile;
        } // End Function ReadFile 


        public static string GetDiff(Repository repo, DiffEntry entry)
        {
            string strDiff = null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                DiffFormatter diffFormatter = new DiffFormatter(ms);
                diffFormatter.SetRepository(repo);
                diffFormatter.Format(diffFormatter.ToFileHeader(entry));

                ms.Position = 0;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(ms))
                {
                    strDiff = sr.ReadToEnd();
                }
            }

            return strDiff;
        } // End Function GetDiff


        // https://stackoverflow.com/questions/13537734/how-to-use-jgit-to-get-list-of-changed-files
        // https://github.com/centic9/jgit-cookbook/blob/master/src/main/java/org/dstadler/jgit/porcelain/ShowChangedFilesBetweenCommits.java
        public static void GetChanges(Git git, Repository repo, RevCommit oldCommit, RevCommit newCommit)
        {
            System.Console.WriteLine("Printing diff between commit: " + oldCommit.ToString() + " and " + newCommit.ToString());
            ObjectReader reader = repo.NewObjectReader();

            // prepare the two iterators to compute the diff between
            CanonicalTreeParser oldTreeIter = new CanonicalTreeParser();
            oldTreeIter.Reset(reader, oldCommit.Tree.Id);
            CanonicalTreeParser newTreeIter = new CanonicalTreeParser();
            newTreeIter.Reset(reader, newCommit.Tree.Id);

            // DiffStatFormatter df = new DiffStatFormatter(newCommit.Name, repo);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {

                DiffFormatter diffFormatter = new DiffFormatter(ms);
                diffFormatter.SetRepository(repo);

                int entryCount = 0;
                foreach (DiffEntry entry in diffFormatter.Scan(oldCommit, newCommit))
                {
                    string pathToUse = null;

                    TreeWalk treeWalk = new TreeWalk(repo);
                    treeWalk.Recursive = true;

                    if (entry.GetChangeType() == DiffEntry.ChangeType.DELETE)
                    {
                        treeWalk.AddTree(oldCommit.Tree);
                        pathToUse = entry.GetOldPath();
                    }
                    else
                    {
                        treeWalk.AddTree(newCommit.Tree);
                        pathToUse = entry.GetNewPath();   
                    }

                    treeWalk.Filter = PathFilter.Create(pathToUse); 
                        
                    if (!treeWalk.Next())
                    {
                        throw new System.Exception("Did not find expected file '" + pathToUse + "'");
                    }

                    ObjectId objectId = treeWalk.GetObjectId(0);
                    ObjectLoader loader = repo.Open(objectId);

                    string strModifiedFile = ReadFile(loader);
                    System.Console.WriteLine(strModifiedFile);

                    //////////////
                    // https://stackoverflow.com/questions/27361538/how-to-show-changes-between-commits-with-jgit
                    diffFormatter.Format(diffFormatter.ToFileHeader(entry));

                    string diff = GetDiff(repo, entry);
                    System.Console.WriteLine(diff);

                    entryCount++;
                } // Next entry 

                System.Console.WriteLine(entryCount);

                ms.Position = 0;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(ms))
                {
                    string strAllDiffs = sr.ReadToEnd();
                    System.Console.WriteLine(strAllDiffs);
                }
                
            }


            System.Collections.Generic.IList<DiffEntry> diffs = git.Diff()
                             .SetNewTree(newTreeIter)
                             .SetOldTree(oldTreeIter)
                             .Call();

            foreach (DiffEntry entry in diffs)
            {
                System.Console.WriteLine("Entry: " + entry);
                System.Console.WriteLine("Entry: " + entry.GetChangeType());
            }

            System.Console.WriteLine("Done");
        }


        // https://github.com/mono/ngit/commits/master
        public static void getCommitsByTree(string treeName)
        {

            string dir = @"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\ngit";
            dir = "/root/sources/GitRevisionControl";
            // dir = "https://github.com/mono/ngit.git";
            // https://github.com/centic9/jgit-cookbook/blob/master/src/main/java/org/dstadler/jgit/porcelain/ListRemoteRepository.java
            // https://stackoverflow.com/questions/13667988/how-to-use-ls-remote-in-ngit
            // git.LsRemote();


            Git git = Git.Open(dir);
            Repository repo = git.GetRepository();


            // https://stackoverflow.com/questions/15822544/jgit-how-to-get-all-commits-of-a-branch-without-changes-to-the-working-direct
            ObjectId oidTheBranch = repo.Resolve(treeName);
            System.Console.WriteLine(oidTheBranch);

            // Get All Branches
            // https://github.com/centic9/jgit-cookbook/blob/master/src/main/java/org/dstadler/jgit/porcelain/ListBranches.java
            System.Collections.Generic.IList<Ref> branchList =
                git.BranchList().SetListMode(ListBranchCommand.ListMode.ALL).Call();

            foreach (Ref branch in branchList)
            {
                string branchName = branch.GetName();

                if (!string.Equals(branchName, Constants.R_HEADS + treeName, System.StringComparison.InvariantCultureIgnoreCase))
                    continue;

                ObjectId oid = branch.GetObjectId();
                System.Console.WriteLine("Commits of branch: " + branchName);
                System.Console.WriteLine("-------------------------------------");


                Sharpen.Iterable<RevCommit> commits = git.Log().Add(oid).Call();

                int count = 0;

                RevCommit laterCommit = null;

                // Note: Apparently sorted DESCENDING by COMMIT DATE
                foreach (RevCommit earlierCommit in commits)
                {
                    System.Console.WriteLine(earlierCommit.Name);
                    System.Console.WriteLine(earlierCommit.GetAuthorIdent().GetName());

                    // System.DateTime dt = new System.DateTime(commit.CommitTime);
                    System.DateTime dt = UnixTimeStampToDateTime(earlierCommit.CommitTime);

                    System.Console.WriteLine(dt);
                    System.Console.WriteLine(earlierCommit.GetFullMessage());

                    if (laterCommit != null)
                    {
                        GetChanges(git, repo, earlierCommit, laterCommit);
                    }


                    System.Console.WriteLine(earlierCommit.Tree);

                    // https://github.com/gitblit/gitblit/blob/master/src/main/java/com/gitblit/utils/JGitUtils.java#L718

                    laterCommit = earlierCommit;
                    count++;
                }
                System.Console.WriteLine(count);
            }

            // https://github.com/mono/ngit/blob/master/NGit/NGit.Revwalk/RevWalkUtils.cs
        }

        // https://stackoverflow.com/questions/15822544/jgit-how-to-get-all-commits-of-a-branch-without-changes-to-the-working-direct
        public static void WalkCommits()
        {
            Git git = Git.Open(@"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\ngit");
            Repository repo = git.GetRepository();

            RevWalk walk = new RevWalk(repo);




            System.Collections.Generic.IList<Ref> branches = git.BranchList().Call();

            // https://stackoverflow.com/questions/15822544/jgit-how-to-get-all-commits-of-a-branch-without-changes-to-the-working-direct
            foreach (Ref branch in branches)
            {
                string branchName = branch.GetName();
                System.Console.WriteLine("Commits of branch: " + branchName);
                System.Console.WriteLine("-------------------------------------");

                Sharpen.Iterable<RevCommit> commits = git.Log().All().Call();

                foreach (RevCommit commit in commits)
                {
                    bool foundInThisBranch = false;

                    RevCommit targetCommit = walk.ParseCommit(repo.Resolve(commit.Name));

                    foreach (System.Collections.Generic.KeyValuePair<string, Ref> e in repo.GetAllRefs())
                    {

                        if (e.Key.StartsWith(Constants.R_HEADS))
                        {

                            if (walk.IsMergedInto(targetCommit, walk.ParseCommit(e.Value.GetObjectId())))
                            {
                                string foundInBranch = e.Value.GetName();

                                if (branchName.Equals(foundInBranch))
                                {
                                    foundInThisBranch = true;
                                    break;
                                }
                            } // End if (walk.IsMergedInto(targetCommit, walk.ParseCommit(e.Value.GetObjectId())))

                        } // End if (e.Key.StartsWith(Constants.R_HEADS)) 

                    } // Next e

                    if (foundInThisBranch)
                    {
                        System.Console.WriteLine(commit.Name);
                        System.Console.WriteLine(commit.GetAuthorIdent().GetName());

                        // System.DateTime dt = new System.DateTime(commit.CommitTime);
                        System.DateTime dt = UnixTimeStampToDateTime(commit.CommitTime);

                        System.Console.WriteLine(dt);
                        System.Console.WriteLine(commit.GetFullMessage());
                    } // End if (foundInThisBranch) 

                } // Next commit 

            } // Next branch 

        }
        // End Sub


        public static System.DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        public static void TestClone()
        {
            // Let's clone the NGit repository
            var clone = Git.CloneRepository()
                .SetDirectory(@"C:\Git\NGit")
                .SetURI("https://github.com/mono/ngit.git");

            // Execute and return the repository object we'll use for further commands
            var repository = clone.Call();
        }


        public static void OpenRepo()
        {
            var repository = Git.Open(@"C:\Git\NGit");

            // Fetch changes without merging them
            var fetch = repository.Fetch().Call();

            // Pull changes (will automatically merge/commit them)
            var pull = repository.Pull().Call();

            // Get the current branch status
            var status = repository.Status().Call();

            // The IsClean() method is helpful to check if any changes
            // have been detected in the working copy. I recommend using it,
            // as NGit will happily make a commit with no actual file changes.
            bool isClean = status.IsClean();

            // You can also access other collections related to the status
            var added = status.GetAdded();
            var changed = status.GetChanged();
            var removed = status.GetRemoved();

            // Clean our working copy
            var clean = repository.Clean().Call();

            // Add all files to the stage (you could also be more specific)
            var add = repository.Add().AddFilepattern(".").Call();

            // Remove files from the stage
            var remove = repository.Rm().AddFilepattern(".gitignore").Call();
        }


        public static void Reset()
        {
            var repository = Git.Open(@"C:\Git\NGit");

            var reset = repository.Reset()
    .SetMode(ResetCommand.ResetType.HARD)
    .SetRef("origin/master")
    .Call();
        }

        public static void Commit()
        {
            var repository = Git.Open(@"C:\Git\NGit");
            var author = new PersonIdent("Lance Mcnearney", "lance@mcnearney.net");
            var message = "My commit message";

            // Commit our changes after adding files to the stage
            var commit = repository.Commit()
                .SetMessage(message)
                .SetAuthor(author)
                .SetAll(true) // This automatically stages modified and deleted files
                .Call();

            // Our new commit's hash
            var hash = commit.Id;

            // Push our changes back to the origin
            var push = repository.Push().Call();


            // Handle disposing of NGit's locks
            repository.GetRepository().Close();
            repository.GetRepository().ObjectDatabase.Close();
            repository = null;
        }


        public static void RemoveWriteProtection()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@"C:\Git\NGit");

            System.IO.FileInfo[] files = di.GetFiles("*", System.IO.SearchOption.AllDirectories);

            // Remove the read-only attribute applied by NGit to some of its files
            foreach (System.IO.FileInfo file in files)
            {
                file.Attributes = System.IO.FileAttributes.Normal;
            }
        }


        public static void WithCreds()
        {
            dynamic repository = null;

            var credentials = new NGit.Transport.UsernamePasswordCredentialsProvider("username", "password");

            // On a per-command basis
            var fetch = repository.Fetch()
                .SetCredentialsProvider(credentials)
                .Call();

            // Or globally as the default for each new command
            NGit.Transport.CredentialsProvider.SetDefault(credentials);
        }

        public static void TTT(string[] args)
        {
            Git myrepo = Git.Init().SetDirectory(@"/tmp/myrepo.git").SetBare(true).Call();
            {
                var fetchResult = myrepo.Fetch()
                    .SetProgressMonitor(new TextProgressMonitor())
                    .SetRemote(@"/tmp/initial")
                    .SetRefSpecs(new NGit.Transport.RefSpec("refs/heads/master:refs/heads/master"))
                    .Call();
                //
                // Some other work...
                //
                myrepo.GetRepository().Close();
            }
            System.GC.Collect();

#if false
            System.Console.WriteLine("Killing");
            BatchingProgressMonitor.ShutdownNow();
#endif
            System.Console.WriteLine("Done");

        }


    }


}
