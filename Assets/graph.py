import os
import pandas as pd
import matplotlib.pyplot as plt

def read_data_from_file(filnamn):
    """
    Läs data från en textfil och returnera ett DataFrame med relevanta data samt metadata.
    """
    if not os.path.exists(filnamn):
        print(f"Filen '{filnamn}' hittades inte.")
        return None, None, None

    try:
        # Läs alla rader från filen
        with open(filnamn, 'r') as f:
            lines = f.read().strip().split('\n')

        # Initialisera variabler
        pileholder_counts = {}  # Dynamisk räknare för alla spelare som är "Pileholder"
        rounds = []  # För att hålla reda på rundnummer
        meta_data = {}
        points = {}  # Dictionary för att lagra poäng för varje spelare

        # Iterera över varje rad och hämta relevant information
        for line in lines:
            if line.startswith('Time:'):
                meta_data['Time'] = line.split(': ')[1]
            elif line.startswith('Seed:'):
                meta_data['Seed'] = line.split(': ')[1]
            elif line.startswith('Extra Information:'):
                meta_data['Extra Information'] = line.split(': ')[1]
            elif line.startswith('Points:'):
                points_data = list(map(int, line.split(': ')[1].split(',')))

                # Dynamiskt skapa en lista för poäng för varje spelare
                for i, point in enumerate(points_data, start=1):
                    if i not in points:
                        points[i] = []  # Om spelaren inte finns, skapa en lista för den
                    points[i].append(point)

                # Lägg till runda
                rounds.append(len(rounds))

            elif line.startswith('Pileholder:'):
                pileholder = int(line.split(': ')[1])
                if pileholder not in pileholder_counts:
                    pileholder_counts[pileholder] = 0
                pileholder_counts[pileholder] += 1  # Öka räknaren för den spelare som är Pileholder

        # Skapa en DataFrame från poängen varje runda
        data = pd.DataFrame({'Round': rounds})
        for player, player_points in points.items():
            data[f'Player {player}'] = player_points

        # Vänd datan så att den senaste rundan kommer först
        data = data.iloc[::-1].reset_index(drop=True)

        # Uppdatera round-numreringen så att det börjar från 0 efter omvändning
        data['Round'] = range(0, len(data))  # Start from 0 instead of 1

        return data, meta_data, pileholder_counts

    except Exception as e:
        print(f"Fel vid läsning av filen: {e}")
        return None, None, None


def combine_data(files):
    """
    Kombinera data från flera filer och beräkna genomsnittet för varje runda.
    Anpassad för att hantera filer med olika antal rundor.
    """
    all_data = []
    pileholder_counts_total = {}
    min_rounds = None  # För att spåra minsta antal rundor

    # Läs data från varje fil
    for filnamn in files:
        print(f"Bearbetar fil: {filnamn}")
        data, _, pileholder_counts = read_data_from_file(filnamn)
        
        if data is None:
            print(f"Fel vid läsning av filen '{filnamn}'. Hoppar över denna fil.")
            continue

        all_data.append(data)

        # Lägg till pileholder counts
        for player, count in pileholder_counts.items():
            pileholder_counts_total[player] = pileholder_counts_total.get(player, 0) + count

        # Uppdatera minsta antal rundor
        if min_rounds is None or len(data) < min_rounds:
            min_rounds = len(data)

    if not all_data:
        print("Ingen data kunde kombineras från filerna.")
        return None, None

    # Trimma alla dataframes till minsta antal rundor
    trimmed_data = [df.iloc[:min_rounds].reset_index(drop=True) for df in all_data]

    # Beräkna genomsnittet för varje runda för varje spelare
    avg_data = pd.DataFrame({'Round': range(min_rounds)})
    player_count = max(pileholder_counts_total.keys())
    for player in range(1, player_count + 1):
        avg_data[f'Player {player}'] = [
            sum(df[f'Player {player}'][i] for df in trimmed_data) / len(trimmed_data) 
            for i in range(min_rounds)
        ]

    return avg_data, pileholder_counts_total


def plot_graph(data, pileholder_counts):
    """
    Plottar två grafer:
    1. Punktdiagram som visar poängen varje spelare fick just den aktuella rundan.
    2. Stapeldiagram som visar hur många gånger varje spelare har varit "Pileholder".
    """
    # Kontrollera om datan är giltig
    if data is None or data.empty:
        print("Ingen giltig data att plotta.")
        return

    # Skapa ett punktdiagram för varje spelares poäng per runda
    plt.figure(figsize=(14, 8))

    # Plot 1: Poäng per runda
    plt.subplot(2, 1, 1)  # Två rader, en kolumn, första subplot
    for player in data.columns[1:]:  # Dynamiskt hantera spelare
        plt.plot(data['Round'], data[player], marker='o', label=player)

    plt.title('Genomsnittliga poäng för varje spelare per runda')
    plt.xlabel('Rundor')
    plt.ylabel('Genomsnittlig poäng denna runda')
    plt.grid(True)
    plt.legend()  # Lägg till en legend i diagrammet
    plt.tight_layout()  # Justera layouten för att undvika att etiketter klipps bort

    # Plot 2: Antal gånger varje spelare har varit "Pileholder"
    plt.subplot(2, 1, 2)  # Två rader, en kolumn, andra subplot
    players = [f'Spelare {i}' for i in pileholder_counts.keys()]
    pileholder_values = list(pileholder_counts.values())
    plt.bar(players, pileholder_values, color=['blue', 'orange', 'green', 'red'])

    plt.title('Antal gånger varje spelare har varit Pileholder')
    plt.xlabel('Spelare')
    plt.ylabel('Antal gånger som Pileholder')
    plt.grid(axis='y')

    # Justera layouten igen för att undvika överlappning
    plt.tight_layout()

    # Visa alla diagram
    plt.show()


def main():
    # Fråga användaren om vilka filer som ska användas
    file_names = input("Ange filnamn (separera med kommatecken för flera filer): ").split(',')
    
    # Ta bort extra mellanslag från filnamn
    file_names = [file.strip() for file in file_names]
    print(f"Filer som kommer att läsas in: {file_names}")

    # Kontrollera att alla filer finns innan vi fortsätter
    for file in file_names:
        if not os.path.exists(file):
            print(f"Filen '{file}' finns inte. Kontrollera sökvägen.")
            return  # Avsluta om någon fil inte finns

    # Kombinera data från alla filer och beräkna genomsnitt
    avg_data, pileholder_counts_total = combine_data(file_names)

    if avg_data is not None:
        # Skriv ut genomsnittlig data i konsolen
        print("\nGenomsnittlig poäng per runda:")
        print(avg_data)

        # Plotta diagrammen
        plot_graph(avg_data, pileholder_counts_total)
    else:
        print("Det gick inte att kombinera data från filerna.")


if __name__ == "__main__":
    main()