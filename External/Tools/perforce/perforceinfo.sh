#!/bin/bash

if [ $# -eq 0 ]; then
    # if no arguments are specified
	# default interval is between the start of the month
	# and 23:59:59 of today
	revRange=`date "+@%Y/%m/01:00:00:00,@%Y/%m/%d:23:59:59"`
	#echo No arguments, using default interval: $revRange
else 
	if [ $# -eq 2 ]; then
		revRange=`echo "@$1:00:00:00,@$2:23:59:59"`
		#echo Supplied both dates: $revRange
	else
		# print usage and exit
		progName=`basename $0`
		echo Prints commit stats from perforce during specified interval.
		echo USAGE: $progName START END
		echo where START and END are dates in format YYYY/MM/DD,
		echo e.g. 2010/06/01
		echo
		echo EXAMPLE: $progName 2010/01/01 2010/12/31
		echo will get stats between 00:00:00 January 01, 2010
		echo and 23:59:59 on December 31, 2010
		exit 0
	fi
fi

export revRange

submits=`p4 changes -s submitted $revRange | cut -f6 -d' ' | cut -f1 -d'@'  | sort | uniq -c | sort -nr`

echo Perforce committed changes by user for interval $revRange
echo "$submits"

# print out the users without commits
# as they will not be included in the submits
users=`p4 users | cut -f1 -d' '`
for i in $users; do
	if [[ $submits != *$i* ]]; then
		printf "%7d %s\n" 0 $i
	fi

done