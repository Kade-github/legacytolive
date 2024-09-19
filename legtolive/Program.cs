using System.Text.Json.Serialization;
using LiveFNF;
using MetaNewFNF;
using Newtonsoft.Json;
using OldFNF;
bool hasAddedEvents = false;

void AddNotes(OldFNF.Root root, LiveFNF.Root newRoot, int diff) {

  switch (diff) {
  case 1:
    newRoot.notes.easy = new List < Easy > ();
    break;
  case 2:
    newRoot.notes.normal = new List < Normal > ();
    break;
  case 3:
    newRoot.notes.hard = new List < Hard > ();
    break;
  }

  OldFNF.Note lastNote = null;

  foreach(var note in root.song.notes) {
    if (!hasAddedEvents) {
      if (lastNote != null && note.sectionNotes.Count != 0) {
        if (note.mustHitSection) {
          // add focus event

          newRoot.events.Add(new Event() {
            t = Convert.ToInt32(note.sectionNotes[0][0]), e = "FocusCamera",
              v = new V() {
                @char = 0, d = 4, ease = "CLASSIC"
              }
          });
        } else {
          // add focus event

          newRoot.events.Add(new Event() {
            t = Convert.ToInt32(note.sectionNotes[0][0]), e = "FocusCamera",
              v = new V() {
                @char = 1, d = 4, ease = "CLASSIC"
              }
          });
        }
      }
    }

    foreach(var section in note.sectionNotes) {
      var c = section[1];

      int column = Convert.ToInt32(c);

      if (!note.mustHitSection) {
        if (column >= 4)
          column -= 4;
        else
          column = 7 - column;
      }
      switch (diff) {
      case 1:
        newRoot.notes.easy.Add(new Easy() {
          d = column, t = Convert.ToInt32(section[0]), l = Convert.ToInt32(section[2])
        });
        break;
      case 2:
        newRoot.notes.normal.Add(new Normal() {
          d = column, t = Convert.ToInt32(section[0]), l = Convert.ToInt32(section[2])
        });
        break;
      case 3:
        newRoot.notes.hard.Add(new Hard() {
          d = column, t = Convert.ToInt32(section[0]), l = Convert.ToInt32(section[2])
        });
        break;
      }
    }

    lastNote = note;
  }

  hasAddedEvents = true;
}

void Convertt() {
  hasAddedEvents = false;
  Console.Clear();
  Console.WriteLine("Type custom diffs (separated by commas, max 3. ex: \",reactive,funky\". First comma empty because theres no easy diff in weekend hex.), or type nothing for default diffs (easy, normal, hard)");

  List < String > customDiffs = new List < string > ();

  string customDiff = Console.ReadLine();

  if (customDiff.Length != 0) {
    customDiffs = customDiff.Split(",").ToList();
  }

  customDiff = customDiff.ToLower();

  Console.WriteLine("Input chart paths:");

  string path = Console.ReadLine();

  string[] files = Directory.GetFiles(path, "*.json");

  string first = "";
  string second = "";
  string third = "";

  string songName = "";

  foreach(var file in files) {
    if (file.Contains("easy")) {
      first = file;
      songName = Path.GetFileNameWithoutExtension(file);
    } else if (file.Contains("normal")) {
      second = file;
      songName = Path.GetFileNameWithoutExtension(file);
    } else if (file.Contains("hard")) {
      third = file;
      songName = Path.GetFileNameWithoutExtension(file);
    }
  }

  if (first.Length == 0 && second.Length == 0 && third.Length == 0) // modded diffs
  {
    if (files.Length == 0) {
      Console.WriteLine("Missing difficulty files");
      Console.Read();
      return;
    } else {
      if (customDiffs.Count == 0) {
        Console.WriteLine("Missing custom diffs");
        Console.Read();
        return;
      }
      foreach(var file in files) {
        if (!file.Contains("-"))
          continue;

        string noExt = Path.GetFileNameWithoutExtension(file);
        string diff = noExt.Split("-").LastOrDefault();

        if (customDiffs.Contains(diff)) {
          int indexOfDiff = customDiffs.IndexOf(diff);

          if (indexOfDiff == 0)
            first = file;
          else if (indexOfDiff == 1)
            second = file;
          else if (indexOfDiff == 2)
            third = file;

          songName = noExt;
        }

      }
    }
  }

  if (songName.Contains("-"))
    songName = songName.Substring(0, songName.LastIndexOf("-"));

  Console.WriteLine("reading: " + first);

  var old1 = new OldFNF.Root();
  old1.song = new OldFNF.Song();
  old1.song.notes = new List < Note > ();
  if (first != "")
    old1 = JsonConvert.DeserializeObject < OldFNF.Root > (File.ReadAllText(first));
  Console.WriteLine("reading: " + second);

  var old2 = new OldFNF.Root();
  old2.song = new OldFNF.Song();
  old2.song.notes = new List < Note > ();
  if (second != "")
    old2 = JsonConvert.DeserializeObject < OldFNF.Root > (File.ReadAllText(second));
  Console.WriteLine("reading: " + third);

  var old3 = new OldFNF.Root();
  old3.song = new OldFNF.Song();
  old3.song.notes = new List < Note > ();
  if (third != "")
    old3 = JsonConvert.DeserializeObject < OldFNF.Root > (File.ReadAllText(third));

  // convert old to new

  var newVersion = new LiveFNF.Root();

  newVersion.version = "2.0.0";
  newVersion.generatedBy = "LegacyToLive v1";
  newVersion.scrollSpeed = new ScrollSpeed() {
    normal = (int) old1.song.speed, erect = (int) old2.song.speed, nightmare = (int) old3.song.speed
  };

  newVersion.events = new List < Event > (); // old doesnt have events

  newVersion.notes = new Notes();

  newVersion.notes.easy = new List < Easy > ();

  if (old1.song.notes.Count != 0)
    AddNotes(old1, newVersion, 1); // easy
  else {
    newVersion.notes.easy = new List < Easy > ();
  }
  if (old2.song.notes.Count != 0)
    AddNotes(old2, newVersion, 2); // normal
  else {
    newVersion.notes.normal = new List < Normal > ();
  }
  if (old3.song.notes.Count != 0)
    AddNotes(old3, newVersion, 3); // hard
  else {
    newVersion.notes.hard = new List < Hard > ();
  }
  // output
  File.WriteAllText(songName + "-chart.json", JsonConvert.SerializeObject(newVersion, Formatting.Indented));

  Console.WriteLine("output to " + Path.GetFullPath(songName + "-chart.json"));

  MetaNewFNF.Root mroot = new MetaNewFNF.Root();

  mroot.version = "2.2.4";
  if (songName.Length >= 2) {
    mroot.songName = songName.Replace("-", " ");
    // capitalize each word
    mroot.songName = string.Join(" ", mroot.songName.Split(' ').Select(x => x[0].ToString().ToUpper() + x.Substring(1)));
  } else {
    mroot.songName = songName.Replace("-", " ");
  }

  mroot.artist = "Unknown";
  mroot.charter = "Unknown";
  mroot.divisions = 96;
  mroot.looped = false;
  mroot.generatedBy = "LegacyToLive v1";
  mroot.timeFormat = "ms";
  mroot.timeChanges = new List < MetaNewFNF.TimeChange > ();

  int bpmm = 120;

  if (old1.song.bpm != 0)
    bpmm = old1.song.bpm;
  if (old2.song.bpm != 0)
    bpmm = old2.song.bpm;
  if (old3.song.bpm != 0)
    bpmm = old3.song.bpm;

  mroot.timeChanges.Add(new MetaNewFNF.TimeChange() {
    t = 0, b = 0, bpm = bpmm, n = 4, d = 4, bt = new List < int > () {
      4,
      4,
      4,
      4
    }
  });

  mroot.playData = new MetaNewFNF.PlayData();
  mroot.playData.difficulties = new List < string > () {
    "easy",
    "normal",
    "hard"
  };
  mroot.playData.stage = "mainStage";
  mroot.playData.noteStyle = "funkin";
  mroot.playData.ratings = new MetaNewFNF.Ratings() {
    easy = 1, normal = 2, hard = 3
  };
  mroot.playData.previewStart = 0;
  mroot.playData.previewEnd = 0;

  string player1 = "bf";
  string player2 = "dad";

  if (old1.song.player1 != null) {
    player1 = old1.song.player1;
    player2 = old1.song.player2;
  }

  if (old2.song.player1 != null) {
    player1 = old2.song.player1;
    player2 = old2.song.player2;
  }

  if (old3.song.player1 != null) {
    player1 = old3.song.player1;
    player2 = old3.song.player2;
  }

  mroot.playData.characters = new MetaNewFNF.Characters() {
    player = player1, girlfriend = "gf", opponent = player2, instrumental = "", playerVocals = new List < string > () {
      player1
    }, opponentVocals = new List < string > () {
      player2
    }
  };

  mroot.offsets = new MetaNewFNF.Offsets() {
    instrumental = 0
  };

  File.WriteAllText(songName + "-metadata.json", JsonConvert.SerializeObject(mroot, Formatting.Indented));

  Console.ReadLine();
  Convertt();
}

Convertt();

namespace OldFNF {
  public class Note {
    [JsonProperty("sectionNotes")]
    [JsonPropertyName("sectionNotes")]
    public List < List < object >> sectionNotes {
      get;
      set;
    }

    [JsonProperty("lengthInSteps")]
    [JsonPropertyName("lengthInSteps")]
    public int lengthInSteps {
      get;
      set;
    }

    [JsonProperty("typeOfSection")]
    [JsonPropertyName("typeOfSection")]
    public int typeOfSection {
      get;
      set;
    }

    [JsonProperty("mustHitSection")]
    [JsonPropertyName("mustHitSection")]
    public bool mustHitSection {
      get;
      set;
    }

    [JsonProperty("altAnim")]
    [JsonPropertyName("altAnim")]
    public bool ? altAnim {
      get;
      set;
    }

    [JsonProperty("bpm")]
    [JsonPropertyName("bpm")]
    public int ? bpm {
      get;
      set;
    }

    [JsonProperty("changeBPM")]
    [JsonPropertyName("changeBPM")]
    public bool ? changeBPM {
      get;
      set;
    }
  }

  public class Root {
    [JsonProperty("song")]
    [JsonPropertyName("song")]
    public Song song {
      get;
      set;
    }
  }

  public class Song {
    [JsonProperty("sectionLengths")]
    [JsonPropertyName("sectionLengths")]
    public List < object > sectionLengths {
      get;
      set;
    }

    [JsonProperty("player1")]
    [JsonPropertyName("player1")]
    public string player1 {
      get;
      set;
    }

    [JsonProperty("notes")]
    [JsonPropertyName("notes")]
    public List < Note > notes {
      get;
      set;
    }

    [JsonProperty("player2")]
    [JsonPropertyName("player2")]
    public string player2 {
      get;
      set;
    }

    [JsonProperty("gfVersion")]
    [JsonPropertyName("gfVersion")]
    public string gfVersion {
      get;
      set;
    }

    [JsonProperty("song")]
    [JsonPropertyName("song")]
    public string song {
      get;
      set;
    }

    [JsonProperty("stage")]
    [JsonPropertyName("stage")]
    public string stage {
      get;
      set;
    }

    [JsonProperty("validScore")]
    [JsonPropertyName("validScore")]
    public bool validScore {
      get;
      set;
    }

    [JsonProperty("sections")]
    [JsonPropertyName("sections")]
    public int sections {
      get;
      set;
    }

    [JsonProperty("needsVoices")]
    [JsonPropertyName("needsVoices")]
    public bool needsVoices {
      get;
      set;
    }

    [JsonProperty("speed")]
    [JsonPropertyName("speed")]
    public double speed {
      get;
      set;
    }

    [JsonProperty("bpm")]
    [JsonPropertyName("bpm")]
    public int bpm {
      get;
      set;
    }
  }

}

namespace LiveFNF {
  public class Easy {
    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public int t {
      get;
      set;
    }

    [JsonProperty("l")]
    [JsonPropertyName("l")]
    public int ? l {
      get;
      set;
    }

  }

  public class Normal {
    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public int t {
      get;
      set;
    }

    [JsonProperty("l")]
    [JsonPropertyName("l")]
    public int ? l {
      get;
      set;
    }

  }

  public class Hard {
    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public int t {
      get;
      set;
    }

    [JsonProperty("l")]
    [JsonPropertyName("l")]
    public int ? l {
      get;
      set;
    }

  }
  public class Erect {
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public double t {
      get;
      set;
    }

    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("l")]
    [JsonPropertyName("l")]
    public double l {
      get;
      set;
    }

  }

  public class Event {
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public double t {
      get;
      set;
    }

    [JsonProperty("e")]
    [JsonPropertyName("e")]
    public string e {
      get;
      set;
    }

    [JsonProperty("v")]
    [JsonPropertyName("v")]
    public V v {
      get;
      set;
    }
  }

  public class Nightmare {
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public double t {
      get;
      set;
    }

    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("l")]
    [JsonPropertyName("l")]
    public double l {
      get;
      set;
    }
  }

  public class Notes {
    [JsonProperty("easy")]
    [JsonPropertyName("easy")]
    public List < Easy > easy {
      get;
      set;
    }

    [JsonProperty("normal")]
    [JsonPropertyName("normal")]
    public List < Normal > normal {
      get;
      set;
    }

    [JsonProperty("hard")]
    [JsonPropertyName("hard")]
    public List < Hard > hard {
      get;
      set;
    }
  }

  public class Root {
    [JsonProperty("version")]
    [JsonPropertyName("version")]
    public string version {
      get;
      set;
    }

    [JsonProperty("scrollSpeed")]
    [JsonPropertyName("scrollSpeed")]
    public ScrollSpeed scrollSpeed {
      get;
      set;
    }

    [JsonProperty("events")]
    [JsonPropertyName("events")]
    public List < Event > events {
      get;
      set;
    }

    [JsonProperty("notes")]
    [JsonPropertyName("notes")]
    public Notes notes {
      get;
      set;
    }

    [JsonProperty("generatedBy")]
    [JsonPropertyName("generatedBy")]
    public string generatedBy {
      get;
      set;
    }
  }

  public class ScrollSpeed {
    [JsonProperty("normal")]
    [JsonPropertyName("normal")]
    public int normal {
      get;
      set;
    }

    [JsonProperty("erect")]
    [JsonPropertyName("erect")]
    public double erect {
      get;
      set;
    }

    [JsonProperty("nightmare")]
    [JsonPropertyName("nightmare")]
    public double nightmare {
      get;
      set;
    }
  }

  public class V {
    [JsonProperty("char")]
    [JsonPropertyName("char")]
    public int @char {
      get;
      set;
    }

    [JsonProperty("duration")]
    [JsonPropertyName("duration")]
    public int d {
      get;
      set;
    }

    [JsonProperty("ease")]
    [JsonPropertyName("ease")]
    public string ease {
      get;
      set;
    }
  }
}

namespace MetaNewFNF {

  public class Characters {
    [JsonProperty("player")]
    [JsonPropertyName("player")]
    public string player {
      get;
      set;
    }

    [JsonProperty("girlfriend")]
    [JsonPropertyName("girlfriend")]
    public string girlfriend {
      get;
      set;
    }

    [JsonProperty("opponent")]
    [JsonPropertyName("opponent")]
    public string opponent {
      get;
      set;
    }

    [JsonProperty("instrumental")]
    [JsonPropertyName("instrumental")]
    public string instrumental {
      get;
      set;
    }

    [JsonProperty("opponentVocals")]
    [JsonPropertyName("opponentVocals")]
    public List < string > opponentVocals {
      get;
      set;
    }

    [JsonProperty("playerVocals")]
    [JsonPropertyName("playerVocals")]
    public List < string > playerVocals {
      get;
      set;
    }
  }

  public class Offsets {
    [JsonProperty("instrumental")]
    [JsonPropertyName("instrumental")]
    public int instrumental {
      get;
      set;
    }

  }

  public class PlayData {
    [JsonProperty("difficulties")]
    [JsonPropertyName("difficulties")]
    public List < string > difficulties {
      get;
      set;
    }

    [JsonProperty("characters")]
    [JsonPropertyName("characters")]
    public Characters characters {
      get;
      set;
    }

    [JsonProperty("stage")]
    [JsonPropertyName("stage")]
    public string stage {
      get;
      set;
    }

    [JsonProperty("noteStyle")]
    [JsonPropertyName("noteStyle")]
    public string noteStyle {
      get;
      set;
    }

    [JsonProperty("ratings")]
    [JsonPropertyName("ratings")]
    public Ratings ratings {
      get;
      set;
    }

    [JsonProperty("previewStart")]
    [JsonPropertyName("previewStart")]
    public int previewStart {
      get;
      set;
    }

    [JsonProperty("previewEnd")]
    [JsonPropertyName("previewEnd")]
    public int previewEnd {
      get;
      set;
    }
  }

  public class Ratings {
    [JsonProperty("easy")]
    [JsonPropertyName("easy")]
    public int easy {
      get;
      set;
    }

    [JsonProperty("normal")]
    [JsonPropertyName("normal")]
    public int normal {
      get;
      set;
    }

    [JsonProperty("hard")]
    [JsonPropertyName("hard")]
    public int hard {
      get;
      set;
    }

    [JsonProperty("erect")]
    [JsonPropertyName("erect")]
    public int erect {
      get;
      set;
    }

    [JsonProperty("nightmare")]
    [JsonPropertyName("nightmare")]
    public int nightmare {
      get;
      set;
    }
  }

  public class Root {
    [JsonProperty("version")]
    [JsonPropertyName("version")]
    public string version {
      get;
      set;
    }

    [JsonProperty("songName")]
    [JsonPropertyName("songName")]
    public string songName {
      get;
      set;
    }

    [JsonProperty("artist")]
    [JsonPropertyName("artist")]
    public string artist {
      get;
      set;
    }

    [JsonProperty("charter")]
    [JsonPropertyName("charter")]
    public string charter {
      get;
      set;
    }

    [JsonProperty("divisions")]
    [JsonPropertyName("divisions")]
    public int divisions {
      get;
      set;
    }

    [JsonProperty("looped")]
    [JsonPropertyName("looped")]
    public bool looped {
      get;
      set;
    }

    [JsonProperty("offsets")]
    [JsonPropertyName("offsets")]
    public Offsets offsets {
      get;
      set;
    }

    [JsonProperty("playData")]
    [JsonPropertyName("playData")]
    public PlayData playData {
      get;
      set;
    }

    [JsonProperty("generatedBy")]
    [JsonPropertyName("generatedBy")]
    public string generatedBy {
      get;
      set;
    }

    [JsonProperty("timeFormat")]
    [JsonPropertyName("timeFormat")]
    public string timeFormat {
      get;
      set;
    }

    [JsonProperty("timeChanges")]
    [JsonPropertyName("timeChanges")]
    public List < TimeChange > timeChanges {
      get;
      set;
    }
  }

  public class TimeChange {
    [JsonProperty("t")]
    [JsonPropertyName("t")]
    public int t {
      get;
      set;
    }

    [JsonProperty("b")]
    [JsonPropertyName("b")]
    public int b {
      get;
      set;
    }

    [JsonProperty("bpm")]
    [JsonPropertyName("bpm")]
    public int bpm {
      get;
      set;
    }

    [JsonProperty("n")]
    [JsonPropertyName("n")]
    public int n {
      get;
      set;
    }

    [JsonProperty("d")]
    [JsonPropertyName("d")]
    public int d {
      get;
      set;
    }

    [JsonProperty("bt")]
    [JsonPropertyName("bt")]
    public List < int > bt {
      get;
      set;
    }
  }

}