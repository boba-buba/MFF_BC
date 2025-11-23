import sys
import csv
#file path /home/_teaching/para/04-spark/spark/opt/data/seznam.csv
#internal opt/data/seznam.csv
from pyspark.sql import SparkSession
from pyspark.sql.functions import col, sum as spark_sum
from pyspark.sql.functions import substring, concat_ws
from pyspark.sql.functions import desc, asc
from pyspark import SparkContext
from pyspark.sql import SparkSession
from pyspark.conf import SparkConf
from pyspark.sql.types import StructType, StructField
from pyspark.sql.types import IntegerType, StringType
from pyspark.sql.functions import concat_ws, col, lit, substring
from pyspark.sql.functions import sum, count
import operator


import pyspark.sql.functions as sf
internal_path_input = "/opt/data/seznam.csv"
internal_output_path = "/mnt/1/output.csv"

#region,num_collisions ... ascending order
####################
internal_output_path = "output.csv"
internal_path_input = "names.csv"

# spark = SparkSession \
#     .builder \
#     .appName("CollisonCompute") \
#     .getOrCreate()



# df = spark.read.csv(path, header=False, sep=',')

################1st###########
# prochazet a plnit slovnik

# regions = dict()
# result = {1 : 0, 2: 0, 3: 0, 4: 0, 5: 0, 6: 0, 7: 0}

# for i in range(1, 8):
#     print(i)
#     new_df = df.filter(col("_c3").startswith(str(i)))
#     regions[i] = new_df
#     regions[i].show()
#     comp_df = new_df.groupBy("_c0", "_c1").count()

#     filtered_df = comp_df.filter(col("count") > 1)

#     # Sum the values in the count column
#     sum_of_counts = filtered_df.agg(spark_sum("count")).collect()[0][0]
#     result[i] = sum_of_counts

# print(result)

# spark.stop()

###2nd###############

# region_col = df.groupBy(sf.substring(col("_c3"), 0, 1).alias("r"), "_c0", "_c1").count()
# filtered_df = region_col.filter(col("count") > 1)

# computed_sums = filtered_df.groupBy("r").sum("count")

# result = computed_sums.orderBy(sf.asc("r"))
# result.show()

# result.write.csv(output_path)

####3rd############
# from pyspark.sql.functions import substring, concat_ws
# df = df.withColumn("Region", substring(df["_c3"], 0, 1))
# df = df.withColumn("FullName", concat_ws(" ", df["_c0"], df["_c1"]))
# #df.show()


# counts = df.groupBy("Region", "FullName").count()
# #counts.show()

# non_unique_counts = counts.filter("count > 1")
# #non_unique_counts.show()
# result = non_unique_counts.groupBy("Region").sum("count")
# #result.show()

# result.write.csv(output_path)
# spark.stop()

###4th###################

# from pyspark.sql.functions import substring, concat_ws
# df = df.withColumn("Region", substring(df["_c3"], 0, 1))

# partitioned = df.repartition(7, "Region")

# def func(itr):
#     counts = spark.createDataFrame(itr).groupBy("_c0", "_c1").count()
#     counts.show()

# partitioned.foreachPartition(func)

#######5th#################
spark = SparkSession \
    .builder \
    .appName("CollisonCompute") \
    .getOrCreate()


df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

with_region_df = df.withColumn("Region", substring(df["_c3"], 0, 1)) \
    .groupBy("Region", "_c0", "_c1").count() \
    .filter("count > 1") \
    .groupBy("Region").sum("count") \
    .sort("Region").collect()

# spark.stop()

# with open(internal_output_path, 'w', newline='\n') as f:
#     writer = csv.writer(f, delimiter=',')
#     for row in with_region_df:
#         writer.writerow(row)

#######6th#####################

# schema = StructType([ StructField("Name", StringType()), StructField("Surname", StringType()), StructField("Number", IntegerType()), StructField("Postcode", IntegerType()) ]) 
# #path = "/opt/data/seznam.csv" 
# #path = "/mnt/1/seznam.csv" 
# #save_path = "/mnt/1/output.csv" 
# spark_conf = SparkConf()
# #spark_conf.set('spark.sql.files.maxPartitionBytes', '1000mb')
# spark = SparkSession \
#     .builder \
#     .config(conf=spark_conf) \
#     .appName('citizens-count') \
#     .getOrCreate()


# data = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED", schema = schema )
# res = data.withColumn("Region", data.Postcode.substr(0, 1)) \
#         .withColumn("FullNameReg", concat_ws(" ", col("Name"), col("Surname"), col("Region"))) \
#         .select('FullNameReg').withColumn("Count", lit(1).cast(IntegerType())) \
#         .groupBy("FullNameReg").agg(_sum("Count").alias("Count")).filter("Count > 1") \
#         .withColumn("Reg", substring(col("FullNameReg"), -1, 1)) \
#         .select("Reg", "Count") \
#         .groupBy("Reg") \
#         .agg(_sum("Count").alias("Count")) \
#         .sort("Reg").collect()

# file = open(internal_output_path,'w')
# for item in res:
#     file.write(str(item[0])+","+str(item[1])+"\n") 
# file.close() 
# spark.stop()


# spark = SparkSession \
#     .builder \
#     .appName("CollisonCompute") \
#     .getOrCreate()


# #schema = StructType([ StructField("Name", StringType()), StructField("Surname", StringType()), StructField("Number", IntegerType()), StructField("Postcode", IntegerType()) ]) 
# df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

# region_full_name = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
#     .withColumn("RNS", concat_ws(" ", col("Region"), col("_c0"), col("_c1"))) \
#     .withColumn("Number", lit(1).cast(IntegerType())) \
#     .select(col("RNS"), col("Number"))



#  #.groupBy(col("RNS")).agg(sf.sum("Number")) \
#     # .filter("sum(Number) > 1") \
#     # .withColumn("Region", substring("RNS", 0, 1)) \
#     # .groupBy(col("Region")).agg(sf.sum("sum(Number)")) \
#     # .sort(asc(col("Region"))) \
#     # .collect()

# spark.stop()


# with open(internal_output_path, 'w') as f:
#     for row in with_region_df:
#         f.write(row[0] + ',' + str(row[1]) + '\n')

########################new######################

# spark = SparkSession \
#     .builder \
#     .appName('CollisonCompute') \
#     .getOrCreate()

# sc = spark.sparkContext


# df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

# with_region_df = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
#     .withColumn("RNS", concat_ws(" ", col("_c0"), col("_c1"))) \
#     .select(col("Region"), col("RNS"))


# with_region_df.show()


# rdd2 = rdd_region.collect()

##################smth#########################
#results = data.rdd.map(lambda row: (row._c3[0], 1)).reduceByKey(operator.add).collect() # only counts number of people per region without finding duplicates



# df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

# with_region_df = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
#                 .groupBy("_c0", "_c1", "Region").agg(count("*").alias("Count"))

# filtered_df = with_region_df.filter(col("Count") > 1) \
#             .groupBy(col("Region")).agg(sum(col("Count"))) \
#             .sort(asc(col("Region")))

# result = filtered_df.collect()
####################new#####################33
# spark = SparkSession \
#     .builder \
#     .appName("CollisonCompute") \
#     .getOrCreate()


# data = spark.read.csv(internal_path_input, header=False, sep=",")

# with_region_df = data.rdd.map(lambda row: (row._c3[0] + row._c0 + row._c1, 1)).reduceByKey(operator.add)

# with_region_df = with_region_df.filter(lambda x: x[1] > 1)

# with_region_df = with_region_df.map(lambda x: (x[0][0], x[1])).reduceByKey(operator.add).sortBy(lambda x: x[0][0]).collect()


##########

spark.stop()

with open(internal_output_path, 'w') as f:
    for row in with_region_df:
        f.write(row[0] + ',' + str(row[1]) + '\n')

