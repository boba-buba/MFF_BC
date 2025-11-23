import sys
import pyspark.sql.functions as sf
from pyspark.sql import SparkSession
from pyspark.conf import SparkConf
from pyspark.sql.types import IntegerType
from pyspark.sql.functions import concat_ws, col, lit, substring, sum, asc, count
import operator

internal_path_input = "/mnt/1/seznam.csv"
internal_output_path = "/mnt/1/output.csv"

spark_conf = SparkConf()
spark = SparkSession \
    .builder \
    .config(conf=spark_conf) \
    .appName('CollisonCompute') \
    .getOrCreate()


#####################1st################################
sc = spark.sparkContext

df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

with_region_df = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
    .withColumn("RNS", concat_ws(" ", col("Region"), col("_c0"), col("_c1"))) \
    .withColumn("Number", lit(1).cast(IntegerType())) \
    .select(col("RNS"), col("Number")).groupBy(col("RNS")).agg(sf.sum("Number")) \
    .filter("sum(Number) > 1") \
    .withColumn("Region", substring("RNS", 0, 1)) \
    .groupBy(col("Region")).agg(sf.sum("sum(Number)")) \
    .sort(sf.asc(col("Region"))) \
    .collect()

####################2nd########################

# with_region_df = df.withColumn("Region", substring(df["_c3"], 0, 1)) \
#     .groupBy("Region", "_c0", "_c1").count() \
#     .filter("count > 1") \
#     .groupBy("Region").sum("count") \
#     .sort("Region").collect()



##################3rd###############################

# df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED")

# with_region_df = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
#                 .groupBy("_c0", "_c1", "Region").agg(count("*").alias("Count"))

# filtered_df = with_region_df.filter(col("Count") > 1) \
#             .groupBy(col("Region")).agg(sum(col("Count"))) \
#             .sort(asc(col("Region")))

# result = filtered_df.collect()

#################4th###############################

data = spark.read.csv(internal_path_input, header=False, sep=",")

d = data.withColumn("Region", substring(col("_c3"), 0, 1)) \
        .withColumn("RNS", concat_ws(" ", col("Region"), col("_c0"), col("_c1"))) \
        .repartition("Region") \
        .select(col("RNS"))

d = d.groupBy("RNS").agg(count("*").alias("Count"))

with_region_df = d.rdd.map(lambda row: (row.RNS[0], 1)).reduce(operator.add).collect()
########################



spark.stop()

with open(internal_output_path, 'w') as f:
    for row in with_region_df:
        f.write(row[0] + ',' + str(row[1]) + '\n')




