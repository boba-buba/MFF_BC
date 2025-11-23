#!/bin/bash

make
echo "1st:"
./levenshtein ../data/01-32k.A ../data/01-32k.B
echo "2nd:"
./levenshtein ../data/02-64k.A ../data/02-64k.B
echo "3rd:"
./levenshtein ../data/03-128k.A ../data/03-128k.B
echo "4th:"
./levenshtein ../data/04-64k.A ../data/04-128k.B
echo "5th:"
./levenshtein ../data/05-128k.A ../data/05-64k.B
