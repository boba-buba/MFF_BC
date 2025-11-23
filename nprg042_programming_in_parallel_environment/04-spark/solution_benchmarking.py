import sys
import csv
from pyspark.sql import SparkSession
from pyspark.sql.functions import concat_ws, col, lit, substring
from pyspark.sql.functions import sum as _sum
from pyspark.sql.functions import desc, asc

from pyspark.sql.functions import substring, concat_ws
from pyspark.sql.types import StructType, StructField
from pyspark.sql.types import IntegerType, StringType

import pyspark.sql.functions as sf
internal_output_path = "output.csv"
internal_path_input = "names.csv"

def test():
    spark = SparkSession \
    .builder \
    .appName("CollisonCompute") \
    .getOrCreate()

    # schema = StructType([ StructField("Name", StringType()), StructField("Surname", StringType()), StructField("Number", IntegerType()), StructField("Postcode", IntegerType()) ]) 
    # df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED", schema = schema)

    # with_region_df = df.withColumn("Region", substring(col("Postcode"), 0, 1)) \
    # .withColumn("RNS", concat_ws(" ", col("Region"), col("Name"), col("Surname"))) \
    # .withColumn("Number", 1) \
    # .select(col("RNS"), col("Number")).groupBy(col("RNS")).agg(sum("Number")) \
    # .filter("count(1) > 1") \
    # .withColumn("Region", substring("RNS", 0, 1)) \
    # .groupBy(col("Region")).sum("count(1)") \
    # .sort(asc(col("Region"))) \
    # .collect()
    # spark.stop()

    # schema = StructType([ StructField("Name", StringType()), StructField("Surname", StringType()), StructField("Number", IntegerType()), StructField("Postcode", IntegerType()) ]) 

    # data = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED", schema = schema )
    # with_region_df = data.withColumn("Region", data.Postcode.substr(0, 1)) \
    #         .withColumn("FullNameReg", concat_ws(" ", col("Name"), col("Surname"), col("Region"))) \
    #         .select('FullNameReg').withColumn("Count", lit(1).cast(IntegerType())) \
    #         .groupBy("FullNameReg").agg(_sum("Count").alias("Count")).filter("Count > 1") \
    #         .withColumn("Reg", substring(col("FullNameReg"), -1, 1)) \
    #         .select("Reg", "Count") \
    #         .groupBy("Reg") \
    #         .agg(_sum("Count").alias("Count")) \
    #         .sort("Reg").collect()

    # df = spark.read.csv(internal_path_input, header = False, mode = "DROPMALFORMED") #, schema = ["Name", "Surname", "Phone", "Postcode"])

    # with_region_df = df.withColumn("Region", substring(col("_c3"), 0, 1)) \
    # .withColumn("RNS", concat_ws(" ", col("Region"), col("_c0"), col("_c1"))) \
    # .withColumn("Number", lit(1).cast(IntegerType())) \
    # .select(col("RNS"), col("Number")).groupBy(col("RNS")).agg(sf.sum("Number")) \
    # .filter("sum(Number) > 1") \
    # .withColumn("Region", substring("RNS", 0, 1)) \
    # .groupBy(col("Region")).agg(sf.sum("sum(Number)")) \
    # .sort(asc(col("Region"))) \
    # .collect()

    spark.stop()


    with open(internal_output_path, 'w', newline='\n') as f:
        writer = csv.writer(f, delimiter=',')
        for row in with_region_df:
            writer.writerow(row)


if __name__ == '__main__':
    import timeit
    # For Python>=3.5 one can also write:
    print(timeit.timeit("test()", number=10, globals=locals()))
