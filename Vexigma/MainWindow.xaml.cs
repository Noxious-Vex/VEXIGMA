using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Vexigma
{
    public partial class MainWindow : Window
    {
        // =========================================================
        // VEXIGMA VERSION INFORMATION
        // =========================================================

        private const string MachineVersion =
            "VEXIGMA|MACHINE|V1|";


        // =========================================================
        // CHARACTER SETS
        // =========================================================

        private const string Letters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            " " +
            "\r" +
            "\n";

        private const string LettersCaseSensitive =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            " " +
            "\r" +
            "\n";

        private const string LettersAndNumbers =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789" +
            " " +
            "\r" +
            "\n";

        private const string LettersAndNumbersCaseSensitive =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789" +
            " " +
            "\r" +
            "\n";

        private const string AllCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789" +
            "!@#$%^&*()_+-=" +
            "[]{}\\|;:'\",.<>/?`~" +
            " " +
            "\r" +
            "\n";


        // =========================================================
        // CHARACTER MODE
        // =========================================================

        private enum CharacterMode
        {
            Letters = 0,
            LettersCaseSensitive = 1,
            LettersAndNumbers = 2,
            LettersAndNumbersCaseSensitive = 3,
            AllCharacters = 4
        }


        // =========================================================
        // STEPPING MODE
        // =========================================================

        private enum SteppingMode
        {
            Standard = 0,
            Sequential = 1
        }


        // =========================================================
        // ROTOR COUNT OPTIONS
        // =========================================================

        private static readonly int[] RotorCountOptions =
        {
            3,
            4,
            5,
            6,
            8,
            10
        };


        // =========================================================
        // KEY STATE
        // =========================================================

        private bool _keyIsVisible = false;

        private bool _isSynchronizingKeyFields = false;

        private const string KeyCharacters =
            "ABCDEFGHJKLMNPQRSTUVWXYZ" +
            "abcdefghijkmnopqrstuvwxyz" +
            "23456789";

        private const char EncryptedSpaceDisplayCharacter =
            '✯';

        private const char EncryptedCarriageReturnDisplayCharacter =
            '★';

        private const char EncryptedLineFeedDisplayCharacter =
            '✩';


        // =========================================================
        // MACHINE STATE
        // =========================================================

        private MachineConfiguration? _machineConfiguration;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainWindow()
        {
            InitializeComponent();

            GenerateKeyButton.Click +=
                GenerateKeyButton_Click;

            CopyKeyButton.Click +=
                CopyKeyButton_Click;

            ShowKeyButton.Click +=
                ShowKeyButton_Click;

            ConfigureMachineButton.Click +=
                ConfigureMachineButton_Click;

            EncryptButton.Click +=
                EncryptButton_Click;

            DecryptButton.Click +=
                DecryptButton_Click;

            ClearButton.Click +=
                ClearButton_Click;

            CopyOutputButton.Click +=
                CopyOutputButton_Click;

            StringKeyPasswordBox.PasswordChanged +=
                StringKeyPasswordBox_PasswordChanged;

            VisibleStringKeyTextBox.TextChanged +=
                VisibleStringKeyTextBox_TextChanged;

            CharacterSetComboBox.SelectionChanged +=
                MachineSetting_SelectionChanged;

            RotorCountComboBox.SelectionChanged +=
                MachineSetting_SelectionChanged;

            SteppingModeComboBox.SelectionChanged +=
                MachineSetting_SelectionChanged;
        }


        // =========================================================
        // GENERATE KEY
        // =========================================================

        private void GenerateKeyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string newKey =
                GenerateVexigmaKey();

            _isSynchronizingKeyFields = true;

            StringKeyPasswordBox.Password =
                newKey;

            VisibleStringKeyTextBox.Text =
                newKey;

            _isSynchronizingKeyFields = false;

            InvalidateMachine();

            StatusText.Text =
                "KEY GENERATED - CONFIGURE MACHINE";
        }


        // =========================================================
        // COPY KEY
        // =========================================================

        private void CopyKeyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string key =
                GetCurrentKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                StatusText.Text =
                    "NO KEY TO COPY";

                return;
            }

            try
            {
                Clipboard.SetText(key);

                StatusText.Text =
                    "KEY COPIED TO CLIPBOARD";
            }
            catch
            {
                StatusText.Text =
                    "UNABLE TO COPY KEY";
            }
        }


        // =========================================================
        // SHOW / HIDE KEY
        // =========================================================

        private void ShowKeyButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _isSynchronizingKeyFields = true;

            if (!_keyIsVisible)
            {
                VisibleStringKeyTextBox.Text =
                    StringKeyPasswordBox.Password;

                StringKeyPasswordBox.Visibility =
                    Visibility.Collapsed;

                VisibleStringKeyTextBox.Visibility =
                    Visibility.Visible;

                ShowKeyButton.Content =
                    "◉";

                ShowKeyButton.ToolTip =
                    "Hide the string key";

                _keyIsVisible = true;

                VisibleStringKeyTextBox.Focus();

                VisibleStringKeyTextBox.CaretIndex =
                    VisibleStringKeyTextBox.Text.Length;
            }
            else
            {
                StringKeyPasswordBox.Password =
                    VisibleStringKeyTextBox.Text;

                VisibleStringKeyTextBox.Visibility =
                    Visibility.Collapsed;

                StringKeyPasswordBox.Visibility =
                    Visibility.Visible;

                ShowKeyButton.Content =
                    "👁";

                ShowKeyButton.ToolTip =
                    "Show the string key";

                _keyIsVisible = false;

                StringKeyPasswordBox.Focus();
            }

            _isSynchronizingKeyFields = false;
        }


        // =========================================================
        // KEEP KEY FIELDS SYNCHRONIZED
        // =========================================================

        private void StringKeyPasswordBox_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (_isSynchronizingKeyFields)
            {
                return;
            }

            if (!_keyIsVisible)
            {
                _isSynchronizingKeyFields = true;

                VisibleStringKeyTextBox.Text =
                    StringKeyPasswordBox.Password;

                _isSynchronizingKeyFields = false;
            }

            InvalidateMachine();

            if (IsLoaded)
            {
                StatusText.Text =
                    "KEY CHANGED - CONFIGURE MACHINE";
            }
        }


        private void VisibleStringKeyTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_isSynchronizingKeyFields)
            {
                return;
            }

            if (_keyIsVisible)
            {
                _isSynchronizingKeyFields = true;

                StringKeyPasswordBox.Password =
                    VisibleStringKeyTextBox.Text;

                _isSynchronizingKeyFields = false;
            }

            InvalidateMachine();

            if (IsLoaded)
            {
                StatusText.Text =
                    "KEY CHANGED - CONFIGURE MACHINE";
            }
        }


        // =========================================================
        // MACHINE SETTING CHANGED
        // =========================================================

        private void MachineSetting_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            InvalidateMachine();

            StatusText.Text =
                "MACHINE SETTINGS CHANGED - CONFIGURE MACHINE";
        }


        // =========================================================
        // INVALIDATE MACHINE
        // =========================================================

        private void InvalidateMachine()
        {
            _machineConfiguration = null;
        }


        // =========================================================
        // GET CURRENT KEY
        // =========================================================

        private string GetCurrentKey()
        {
            if (_keyIsVisible)
            {
                return VisibleStringKeyTextBox.Text;
            }

            return StringKeyPasswordBox.Password;
        }


        // =========================================================
        // CRYPTOGRAPHIC KEY GENERATOR
        // =========================================================

        private string GenerateVexigmaKey()
        {
            const int groupCount = 4;
            const int charactersPerGroup = 5;

            StringBuilder key =
                new StringBuilder();

            key.Append("VEX-");

            for (
                int group = 0;
                group < groupCount;
                group++)
            {
                if (group > 0)
                {
                    key.Append("-");
                }

                for (
                    int character = 0;
                    character < charactersPerGroup;
                    character++)
                {
                    int randomIndex =
                        RandomNumberGenerator.GetInt32(
                            KeyCharacters.Length);

                    key.Append(
                        KeyCharacters[randomIndex]);
                }
            }

            return key.ToString();
        }


        // =========================================================
        // CONFIGURE MACHINE
        // =========================================================

        private void ConfigureMachineButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string key =
                GetCurrentKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                StatusText.Text =
                    "ENTER OR GENERATE A STRING KEY";

                return;
            }

            string normalizedKey =
                key.Normalize(
                    NormalizationForm.FormC);

            byte[] machineSeed =
                CreateMachineSeed(
                    normalizedKey);

            CharacterMode characterMode =
                GetSelectedCharacterMode();

            int rotorCount =
                GetSelectedRotorCount();

            SteppingMode steppingMode =
                GetSelectedSteppingMode();

            string activeCharacterSet =
                GetCharacterSet(
                    characterMode);

            List<Rotor> rotors =
                CreateRotors(
                    machineSeed,
                    activeCharacterSet,
                    rotorCount);

            _machineConfiguration =
                new MachineConfiguration
                {
                    Seed =
                        machineSeed,

                    CharacterMode =
                        characterMode,

                    CharacterSet =
                        activeCharacterSet,

                    RotorCount =
                        rotorCount,

                    SteppingMode =
                        steppingMode,

                    Rotors =
                        rotors
                };

            MachineStatusText.Text =
                $"{rotorCount} ROTORS   |   " +
                $"{GetShortCharacterSetName(characterMode)}   |   " +
                $"{GetShortSteppingModeName(steppingMode)}";

            StatusText.Text =
                $"MACHINE CONFIGURED - {rotors.Count} ROTORS READY";
        }


        // =========================================================
        // ENCRYPT
        // =========================================================

        private void EncryptButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_machineConfiguration == null)
            {
                StatusText.Text =
                    "CONFIGURE MACHINE BEFORE ENCRYPTING";

                return;
            }

            string input =
                InputTextBox.Text;

            if (string.IsNullOrEmpty(input))
            {
                OutputTextBox.Clear();

                StatusText.Text =
                    "ENTER TEXT TO ENCRYPT";

                return;
            }

            try
            {
                string encryptedText =
                    TransformMessage(
                        input,
                        _machineConfiguration,
                        decrypt: false);

                OutputTextBox.Text =
                    FormatEncryptedOutput(encryptedText);

                StatusText.Text =
                    "ENCRYPTION COMPLETE";
            }
            catch
            {
                OutputTextBox.Clear();

                StatusText.Text =
                    "ENCRYPTION FAILED";
            }
        }


        // =========================================================
        // DECRYPT
        // =========================================================

        private void DecryptButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_machineConfiguration == null)
            {
                StatusText.Text =
                    "CONFIGURE MACHINE BEFORE DECRYPTING";

                return;
            }

            string input =
                InputTextBox.Text;

            if (string.IsNullOrEmpty(input))
            {
                OutputTextBox.Clear();

                StatusText.Text =
                    "ENTER TEXT TO DECRYPT";

                return;
            }

            try
            {
                string preparedCipherText =
                    PrepareEncryptedInputForDecryption(input);

                OutputTextBox.Text =
                    TransformMessage(
                        preparedCipherText,
                        _machineConfiguration,
                        decrypt: true);

                StatusText.Text =
                    "DECRYPTION COMPLETE";
            }
            catch
            {
                OutputTextBox.Clear();

                StatusText.Text =
                    "DECRYPTION FAILED";
            }
        }


        // =========================================================
        // CLEAR WORKSPACE
        // =========================================================

        private void ClearButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            InputTextBox.Clear();
            OutputTextBox.Clear();

            StatusText.Text =
                "WORKSPACE CLEARED";

            InputTextBox.Focus();
        }


        // =========================================================
        // COPY OUTPUT
        // =========================================================

        private void CopyOutputButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string output =
                OutputTextBox.Text;

            if (string.IsNullOrEmpty(output))
            {
                StatusText.Text =
                    "NO OUTPUT TO COPY";

                return;
            }

            try
            {
                Clipboard.SetText(output);

                StatusText.Text =
                    "OUTPUT COPIED TO CLIPBOARD";
            }
            catch
            {
                StatusText.Text =
                    "UNABLE TO COPY OUTPUT";
            }
        }


        // =========================================================
        // PREPARE FORMATTED CIPHERTEXT FOR DECRYPTION
        // =========================================================

        private static string PrepareEncryptedInputForDecryption(
            string input)
        {
            StringBuilder prepared =
                new StringBuilder(
                    input.Length);

            foreach (char character in input)
            {
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                if (
                    character ==
                    EncryptedSpaceDisplayCharacter)
                {
                    prepared.Append(' ');
                    continue;
                }

                if (
                    character ==
                    EncryptedCarriageReturnDisplayCharacter)
                {
                    prepared.Append('\r');
                    continue;
                }

                if (
                    character ==
                    EncryptedLineFeedDisplayCharacter)
                {
                    prepared.Append('\n');
                    continue;
                }

                prepared.Append(character);
            }

            return prepared.ToString();
        }


        // =========================================================
        // FORMAT ENCRYPTED OUTPUT
        // =========================================================

        private string FormatEncryptedOutput(
            string encryptedText)
        {
            int charactersPerGroup =
                CharactersPerGroupComboBox.SelectedIndex >= 0
                ? CharactersPerGroupComboBox.SelectedIndex + 1
                : 5;

            int groupsPerLine =
                GroupsPerLineComboBox.SelectedIndex >= 0
                ? GroupsPerLineComboBox.SelectedIndex + 1
                : 5;

            StringBuilder formatted =
                new StringBuilder();

            int charactersInGroup = 0;
            int groupsOnLine = 0;

            foreach (char character in encryptedText)
            {
                char displayCharacter =
                    character switch
                    {
                        ' ' =>
                            EncryptedSpaceDisplayCharacter,

                        '\r' =>
                            EncryptedCarriageReturnDisplayCharacter,

                        '\n' =>
                            EncryptedLineFeedDisplayCharacter,

                        _ =>
                            character
                    };

                formatted.Append(displayCharacter);
                charactersInGroup++;

                if (charactersInGroup == charactersPerGroup)
                {
                    charactersInGroup = 0;
                    groupsOnLine++;

                    if (groupsOnLine == groupsPerLine)
                    {
                        formatted.AppendLine();
                        groupsOnLine = 0;
                    }
                    else
                    {
                        formatted.Append(' ');
                    }
                }
            }

            return formatted
                .ToString()
                .TrimEnd();
        }


        // =========================================================
        // TRANSFORM COMPLETE MESSAGE
        // =========================================================

        private static string TransformMessage(
            string input,
            MachineConfiguration machine,
            bool decrypt)
        {
            ResetRotors(
                machine);

            StringBuilder output =
                new StringBuilder(
                    input.Length);

            bool uppercaseMode =
                IsUppercaseMode(
                    machine.CharacterMode);

            foreach (char originalCharacter in input)
            {
                char inputCharacter =
                    originalCharacter;

                if (
                    uppercaseMode &&
                    inputCharacter >= 'a' &&
                    inputCharacter <= 'z')
                {
                    inputCharacter =
                        char.ToUpperInvariant(
                            inputCharacter);
                }

                int inputIndex =
                    machine.CharacterSet.IndexOf(
                        inputCharacter);

                if (inputIndex < 0)
                {
                    output.Append(
                        originalCharacter);

                    continue;
                }

                StepRotors(
                    machine);

                int outputIndex;

                if (decrypt)
                {
                    outputIndex =
                        PassThroughRotorsReverse(
                            inputIndex,
                            machine);
                }
                else
                {
                    outputIndex =
                        PassThroughRotorsForward(
                            inputIndex,
                            machine);
                }

                output.Append(
                    machine.CharacterSet[
                        outputIndex]);
            }

            ResetRotors(
                machine);

            return output.ToString();
        }


        // =========================================================
        // UPPERCASE MODE
        // =========================================================

        private static bool IsUppercaseMode(
            CharacterMode characterMode)
        {
            return
                characterMode ==
                    CharacterMode.Letters ||

                characterMode ==
                    CharacterMode.LettersAndNumbers;
        }


        // =========================================================
        // CREATE MACHINE SEED
        // =========================================================

        private static byte[] CreateMachineSeed(
            string normalizedKey)
        {
            string seedMaterial =
                MachineVersion +
                normalizedKey;

            byte[] seedBytes =
                Encoding.UTF8.GetBytes(
                    seedMaterial);

            return SHA256.HashData(
                seedBytes);
        }


        // =========================================================
        // SELECTED CHARACTER MODE
        // =========================================================

        private CharacterMode GetSelectedCharacterMode()
        {
            return CharacterSetComboBox.SelectedIndex switch
            {
                0 =>
                    CharacterMode.Letters,

                1 =>
                    CharacterMode.LettersCaseSensitive,

                2 =>
                    CharacterMode.LettersAndNumbers,

                3 =>
                    CharacterMode.LettersAndNumbersCaseSensitive,

                4 =>
                    CharacterMode.AllCharacters,

                _ =>
                    CharacterMode.Letters
            };
        }


        // =========================================================
        // SELECTED ROTOR COUNT
        // =========================================================

        private int GetSelectedRotorCount()
        {
            int selectedIndex =
                RotorCountComboBox.SelectedIndex;

            if (
                selectedIndex < 0 ||
                selectedIndex >= RotorCountOptions.Length)
            {
                return RotorCountOptions[0];
            }

            return RotorCountOptions[
                selectedIndex];
        }


        // =========================================================
        // SELECTED STEPPING MODE
        // =========================================================

        private SteppingMode GetSelectedSteppingMode()
        {
            return SteppingModeComboBox.SelectedIndex switch
            {
                0 =>
                    SteppingMode.Standard,

                1 =>
                    SteppingMode.Sequential,

                _ =>
                    SteppingMode.Standard
            };
        }


        // =========================================================
        // CHARACTER SET
        // =========================================================

        private static string GetCharacterSet(
            CharacterMode characterMode)
        {
            return characterMode switch
            {
                CharacterMode.Letters =>
                    Letters,

                CharacterMode.LettersCaseSensitive =>
                    LettersCaseSensitive,

                CharacterMode.LettersAndNumbers =>
                    LettersAndNumbers,

                CharacterMode.LettersAndNumbersCaseSensitive =>
                    LettersAndNumbersCaseSensitive,

                CharacterMode.AllCharacters =>
                    AllCharacters,

                _ =>
                    Letters
            };
        }


        // =========================================================
        // CREATE ROTORS
        // =========================================================

        private static List<Rotor> CreateRotors(
            byte[] machineSeed,
            string characterSet,
            int rotorCount)
        {
            List<Rotor> rotors =
                new List<Rotor>();

            for (
                int rotorIndex = 0;
                rotorIndex < rotorCount;
                rotorIndex++)
            {
                Rotor rotor =
                    CreateRotor(
                        machineSeed,
                        characterSet,
                        rotorIndex);

                rotors.Add(rotor);
            }

            return rotors;
        }


        // =========================================================
        // CREATE INDIVIDUAL ROTOR
        // =========================================================

        private static Rotor CreateRotor(
            byte[] machineSeed,
            string characterSet,
            int rotorIndex)
        {
            DeterministicRandom random =
                new DeterministicRandom(
                    machineSeed,
                    $"ROTOR|{rotorIndex}|V1");

            char[] wiring =
                characterSet.ToCharArray();

            for (
                int i = wiring.Length - 1;
                i > 0;
                i--)
            {
                int swapIndex =
                    random.NextInt(
                        i + 1);

                char temporary =
                    wiring[i];

                wiring[i] =
                    wiring[swapIndex];

                wiring[swapIndex] =
                    temporary;
            }

            int startingPosition =
                random.NextInt(
                    characterSet.Length);

            int turnoverPosition =
                random.NextInt(
                    characterSet.Length);

            return new Rotor
            {
                Index =
                    rotorIndex,

                Wiring =
                    new string(wiring),

                StartingPosition =
                    startingPosition,

                Position =
                    startingPosition,

                TurnoverPosition =
                    turnoverPosition
            };
        }


        // =========================================================
        // RESET ROTORS
        // =========================================================

        private static void ResetRotors(
            MachineConfiguration machine)
        {
            foreach (Rotor rotor in machine.Rotors)
            {
                rotor.Position =
                    rotor.StartingPosition;
            }
        }


        // =========================================================
        // STEP ROTORS
        // =========================================================

        private static void StepRotors(
            MachineConfiguration machine)
        {
            if (machine.Rotors.Count == 0)
            {
                return;
            }

            switch (machine.SteppingMode)
            {
                case SteppingMode.Standard:
                    StepRotorsStandard(
                        machine);
                    break;

                case SteppingMode.Sequential:
                    StepRotorsSequential(
                        machine);
                    break;

                default:
                    StepRotorsStandard(
                        machine);
                    break;
            }
        }


        // =========================================================
        // VEXIGMA STANDARD STEPPING
        // =========================================================

        private static void StepRotorsStandard(
            MachineConfiguration machine)
        {
            int characterCount =
                machine.CharacterSet.Length;

            bool advanceNextRotor =
                true;

            for (
                int rotorIndex = 0;
                rotorIndex < machine.Rotors.Count;
                rotorIndex++)
            {
                if (!advanceNextRotor)
                {
                    break;
                }

                Rotor rotor =
                    machine.Rotors[rotorIndex];

                rotor.Position =
                    Mod(
                        rotor.Position + 1,
                        characterCount);

                advanceNextRotor =
                    rotor.Position ==
                    rotor.TurnoverPosition;
            }
        }


        // =========================================================
        // SEQUENTIAL STEPPING
        // =========================================================

        private static void StepRotorsSequential(
            MachineConfiguration machine)
        {
            int characterCount =
                machine.CharacterSet.Length;

            bool carry =
                true;

            for (
                int rotorIndex = 0;
                rotorIndex < machine.Rotors.Count;
                rotorIndex++)
            {
                if (!carry)
                {
                    break;
                }

                Rotor rotor =
                    machine.Rotors[rotorIndex];

                rotor.Position =
                    Mod(
                        rotor.Position + 1,
                        characterCount);

                carry =
                    rotor.Position == 0;
            }
        }


        // =========================================================
        // FORWARD ROTOR TRANSFORMATION
        // =========================================================

        private static int TransformForward(
            int inputIndex,
            Rotor rotor,
            string characterSet)
        {
            int characterCount =
                characterSet.Length;

            int shiftedInput =
                Mod(
                    inputIndex +
                    rotor.Position,
                    characterCount);

            char wiredCharacter =
                rotor.Wiring[
                    shiftedInput];

            int wiredIndex =
                characterSet.IndexOf(
                    wiredCharacter);

            if (wiredIndex < 0)
            {
                throw new InvalidOperationException(
                    "Rotor wiring contains a character that does not exist " +
                    "in the active character set.");
            }

            return Mod(
                wiredIndex -
                rotor.Position,
                characterCount);
        }


        // =========================================================
        // REVERSE ROTOR TRANSFORMATION
        // =========================================================

        private static int TransformReverse(
            int inputIndex,
            Rotor rotor,
            string characterSet)
        {
            int characterCount =
                characterSet.Length;

            int shiftedInput =
                Mod(
                    inputIndex +
                    rotor.Position,
                    characterCount);

            char targetCharacter =
                characterSet[
                    shiftedInput];

            int wiringIndex =
                rotor.Wiring.IndexOf(
                    targetCharacter);

            if (wiringIndex < 0)
            {
                throw new InvalidOperationException(
                    "The active character could not be located in " +
                    "the rotor wiring.");
            }

            return Mod(
                wiringIndex -
                rotor.Position,
                characterCount);
        }


        // =========================================================
        // PASS THROUGH ROTOR STACK - FORWARD
        // =========================================================

        private static int PassThroughRotorsForward(
            int inputIndex,
            MachineConfiguration machine)
        {
            int currentIndex =
                inputIndex;

            for (
                int rotorIndex = 0;
                rotorIndex < machine.Rotors.Count;
                rotorIndex++)
            {
                currentIndex =
                    TransformForward(
                        currentIndex,
                        machine.Rotors[rotorIndex],
                        machine.CharacterSet);
            }

            return currentIndex;
        }


        // =========================================================
        // PASS THROUGH ROTOR STACK - REVERSE
        // =========================================================

        private static int PassThroughRotorsReverse(
            int inputIndex,
            MachineConfiguration machine)
        {
            int currentIndex =
                inputIndex;

            for (
                int rotorIndex =
                    machine.Rotors.Count - 1;

                rotorIndex >= 0;

                rotorIndex--)
            {
                currentIndex =
                    TransformReverse(
                        currentIndex,
                        machine.Rotors[rotorIndex],
                        machine.CharacterSet);
            }

            return currentIndex;
        }


        // =========================================================
        // MODULO HELPER
        // =========================================================

        private static int Mod(
            int value,
            int modulus)
        {
            int result =
                value % modulus;

            if (result < 0)
            {
                result += modulus;
            }

            return result;
        }


        // =========================================================
        // STATUS DISPLAY HELPERS
        // =========================================================

        private static string GetShortCharacterSetName(
            CharacterMode characterMode)
        {
            return characterMode switch
            {
                CharacterMode.Letters =>
                    "LETTERS",

                CharacterMode.LettersCaseSensitive =>
                    "SENSITIVE LETTERS",

                CharacterMode.LettersAndNumbers =>
                    "LETTERS & NUMBERS",

                CharacterMode.LettersAndNumbersCaseSensitive =>
                    "SENSITIVE LETTERS & NUMBERS",

                CharacterMode.AllCharacters =>
                    "ALL CHARACTERS",

                _ =>
                    "LETTERS"
            };
        }


        private static string GetShortSteppingModeName(
            SteppingMode steppingMode)
        {
            return steppingMode switch
            {
                SteppingMode.Standard =>
                    "STANDARD",

                SteppingMode.Sequential =>
                    "SEQUENTIAL",

                _ =>
                    "STANDARD"
            };
        }


        // =========================================================
        // MACHINE CONFIGURATION CLASS
        // =========================================================

        private sealed class MachineConfiguration
        {
            public byte[] Seed { get; init; } =
                Array.Empty<byte>();

            public CharacterMode CharacterMode { get; init; }

            public string CharacterSet { get; init; } =
                string.Empty;

            public int RotorCount { get; init; }

            public SteppingMode SteppingMode { get; init; }

            public List<Rotor> Rotors { get; init; } =
                new List<Rotor>();
        }


        // =========================================================
        // ROTOR CLASS
        // =========================================================

        private sealed class Rotor
        {
            public int Index { get; init; }

            public string Wiring { get; init; } =
                string.Empty;

            public int StartingPosition { get; init; }

            public int Position { get; set; }

            public int TurnoverPosition { get; init; }
        }


        // =========================================================
        // DETERMINISTIC RANDOM GENERATOR
        // =========================================================

        private sealed class DeterministicRandom
        {
            private readonly byte[] _seed;
            private readonly byte[] _domain;

            private ulong _counter;

            private byte[] _buffer =
                Array.Empty<byte>();

            private int _bufferPosition;


            public DeterministicRandom(
                byte[] seed,
                string domain)
            {
                _seed =
                    (byte[])seed.Clone();

                _domain =
                    Encoding.UTF8.GetBytes(
                        domain);

                _counter = 0;
                _bufferPosition = 0;
            }


            public int NextInt(
                int exclusiveMaximum)
            {
                if (exclusiveMaximum <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(exclusiveMaximum));
                }

                uint maximum =
                    (uint)exclusiveMaximum;

                ulong range =
                    (ulong)uint.MaxValue + 1UL;

                ulong acceptableRange =
                    range -
                    (range % maximum);

                while (true)
                {
                    uint value =
                        NextUInt32();

                    if (
                        (ulong)value <
                        acceptableRange)
                    {
                        return
                            (int)(
                                value %
                                maximum);
                    }
                }
            }


            private uint NextUInt32()
            {
                byte[] bytes =
                    new byte[4];

                for (
                    int i = 0;
                    i < bytes.Length;
                    i++)
                {
                    bytes[i] =
                        NextByte();
                }

                return
                    (uint)(
                        bytes[0] |
                        (bytes[1] << 8) |
                        (bytes[2] << 16) |
                        (bytes[3] << 24));
            }


            private byte NextByte()
            {
                if (
                    _bufferPosition >=
                    _buffer.Length)
                {
                    FillBuffer();
                }

                byte value =
                    _buffer[
                        _bufferPosition];

                _bufferPosition++;

                return value;
            }


            private void FillBuffer()
            {
                byte[] counterBytes =
                    GetCounterBytes(
                        _counter);

                byte[] message =
                    new byte[
                        _domain.Length +
                        counterBytes.Length];

                Buffer.BlockCopy(
                    _domain,
                    0,
                    message,
                    0,
                    _domain.Length);

                Buffer.BlockCopy(
                    counterBytes,
                    0,
                    message,
                    _domain.Length,
                    counterBytes.Length);

                using HMACSHA256 hmac =
                    new HMACSHA256(
                        _seed);

                _buffer =
                    hmac.ComputeHash(
                        message);

                _bufferPosition = 0;

                _counter++;
            }


            private static byte[] GetCounterBytes(
                ulong counter)
            {
                return new byte[]
                {
                    (byte)(counter >> 56),
                    (byte)(counter >> 48),
                    (byte)(counter >> 40),
                    (byte)(counter >> 32),
                    (byte)(counter >> 24),
                    (byte)(counter >> 16),
                    (byte)(counter >> 8),
                    (byte)counter
                };
            }
        }
    }
}