'''
# Sample data with tab-separated words and paragraphs
text_data = """This\tis\tthe\tfirst\tline\tof\ttext.\n\nHere\tis\tthe\tsecond\tline\tin\ta\tnew\tparagraph.\n\t\n\nAnd\tthis\tis\tthe\tthird\tline\tin\tthe\tsame\tparagraph.\n\nLet's\tstart\ta\tnew\tparagraph\twith\tmore\ttab-separated\twords.\n\nWord1\tWord2\tWord3\tWord4\tWord5\n"""

# Save the data to a .txt file
with open("MoreThanOneEmptyLineBetweenParagraphs.txt", "w") as file:
    file.write(text_data)

print("File 'MoreThanOneEmptyLineBetweenParagraphs.txt' has been created.")
'''

'''
# Define the content of the text file
text_data = ""
lines_with_text = 10  # Number of lines with text
empty_lines = 10     # Number of empty lines between lines with text

for line_number in range(1, 36):
    if line_number % (lines_with_text + empty_lines) <= lines_with_text:
        text_data += f"Line {line_number} with text\n"
    else:
        text_data += "\n"

# Save the data to a .txt file
with open("MoreThanOneEmptyLineBetweenParagraphs.txt", "w") as file:
    file.write(text_data)

print("File 'text_file_with_empty_lines.txt' has been created.")
'''

import random
def CreateTable():
    # Number of rows and columns
    num_rows = 10  # You can change this to the desired number of rows
    num_columns = 5

    # Generate random words
    words = ["apple", "banana", "cherry", "date", "fig", "grape", "kiwi", "lemon", "mango", "orange"]

    # Generate the text data
    text_data = ""
    for _ in range(num_rows):
        row = [random.choice(words) for _ in range(num_columns)]
        row_text = " ".join(row)
        text_data += row_text + "\n"

    # Save the data to a .txt file
    with open("NotEqualNumberOfItemsInARowIn.txt", "w") as file:
        file.write(text_data)

    print("File 'text_file_with_columns.txt' has been created.")

import lorem

def CreateParagraphFile():


    # Define the number of paragraphs and the minimum number of words per paragraph
    num_paragraphs = 5  # You can change this to the desired number of paragraphs
    min_words_per_paragraph = 40

    # Generate the text data
    text_data = ""

    for _ in range(num_paragraphs):
        paragraph = ""
        while paragraph.count(" ") < min_words_per_paragraph - 1:
            if paragraph:
                paragraph += " "  # Add space between words
            paragraph += lorem.text()

        text_data += paragraph + "\n\n"

    # Save the data to a .txt file
    with open("WhiteCharsOnTheEOL.txt", "w") as file:
        file.write(text_data)

    print("File 'text_file_with_paragraphs.txt' has been created.")

CreateParagraphFile()