namespace NEventStore.Persistence.RavenPersistence
{
    using System.Collections.Generic;
    using System.Globalization;
    using NEventStore.Serialization;

    public static class ExtensionMethods
    {
        public static string ToRavenCommitId(this Commit commit)
        {
            string id = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", commit.BucketId, commit.StreamId, commit.CommitSequence);
            
            return id;
        }

        public static RavenCommit ToRavenCommit(this Commit commit, IDocumentSerializer serializer)
        {
            return new RavenCommit
            {
                Id = ToRavenCommitId(commit),
                BucketId = commit.BucketId,
                StreamId = commit.StreamId,
                CommitSequence = commit.CommitSequence,
                StartingStreamRevision = commit.StreamRevision - (commit.Events.Count - 1),
                StreamRevision = commit.StreamRevision,
                CommitId = commit.CommitId,
                CommitStamp = commit.CommitStamp,
                Headers = commit.Headers,
                Payload = serializer.Serialize(commit.Events)
            };
        }

        public static Commit ToCommit(this RavenCommit commit, IDocumentSerializer serializer)
        {
            return new Commit(commit.StreamId,
                commit.StreamRevision,
                commit.CommitId,
                commit.CommitSequence,
                commit.CommitStamp,
                commit.Headers,
                serializer.Deserialize<List<EventMessage>>(commit.Payload));
        }

        public static string ToRavenSnapshotId(Snapshot snapshot)
        {
            return string.Format("Snapshots/{0}/{1}/{2}", snapshot.BucketId, snapshot.StreamId, snapshot.StreamRevision);
        }

        public static RavenSnapshot ToRavenSnapshot(this Snapshot snapshot, IDocumentSerializer serializer)
        {
            return new RavenSnapshot
            {
                Id = ToRavenSnapshotId(snapshot),
                BucketId = snapshot.BucketId,
                StreamId = snapshot.StreamId,
                StreamRevision = snapshot.StreamRevision,
                Payload = serializer.Serialize(snapshot.Payload)
            };
        }

        public static Snapshot ToSnapshot(this RavenSnapshot snapshot, IDocumentSerializer serializer)
        {
            if (snapshot == null)
            {
                return null;
            }

            return new Snapshot(snapshot.BucketId, snapshot.StreamRevision, serializer.Deserialize<object>(snapshot.Payload));
        }

        public static RavenStreamHead ToRavenStreamHead(this Commit commit)
        {
            return new RavenStreamHead
            {
                Id = RavenStreamHead.GetStreamHeadId(commit.BucketId, commit.StreamId),
                BucketId = commit.BucketId,
                StreamId = commit.StreamId,
                HeadRevision = commit.StreamRevision,
                SnapshotRevision = 0
            };
        }

        public static RavenStreamHead ToRavenStreamHead(this Snapshot snapshot)
        {
            return new RavenStreamHead
            {
                Id = RavenStreamHead.GetStreamHeadId(snapshot.BucketId, snapshot.StreamId),
                BucketId = snapshot.BucketId,
                StreamId = snapshot.StreamId,
                HeadRevision = snapshot.StreamRevision,
                SnapshotRevision = snapshot.StreamRevision
            };
        }

        public static StreamHead ToStreamHead(this RavenStreamHead streamHead)
        {
            return new StreamHead(streamHead.BucketId, streamHead.StreamId, streamHead.HeadRevision, streamHead.SnapshotRevision);
        }
    }
}