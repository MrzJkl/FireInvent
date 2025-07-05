// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_clothing_variant_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateClothingVariantModel _$CreateClothingVariantModelFromJson(
  Map<String, dynamic> json,
) => CreateClothingVariantModel(
  productId: json['productId'] as String,
  name: json['name'] as String,
  additionalSpecs: json['additionalSpecs'] as String?,
);

Map<String, dynamic> _$CreateClothingVariantModelToJson(
  CreateClothingVariantModel instance,
) => <String, dynamic>{
  'productId': instance.productId,
  'name': instance.name,
  'additionalSpecs': instance.additionalSpecs,
};
